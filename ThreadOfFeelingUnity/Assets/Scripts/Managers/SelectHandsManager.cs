using System;
using System.Diagnostics;
using UnityEngine;

namespace Managers {
    public class SelectHandsManager : MonoBehaviour {
        public static SelectHandsManager Instance { get; private set; }
        private Process _proc;

        // 인스펙터에서 값 변화를 보기 위해 SerializeField 유지
        [SerializeField] 
        private int _currentHand = 0;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            StartPython(); // 시작 시 파이썬 가동
        }

        private void OnDestroy() {
            KillPythonProcess();
            if (Instance == this) Instance = null;
        }
        
        private void OnApplicationQuit() {
            KillPythonProcess();
        }

        private void KillPythonProcess() {
            try {
                if (_proc != null && !_proc.HasExited) {
                    _proc.Kill();
                    _proc.Dispose();
                    _proc = null;
                }
                UnityEngine.Debug.Log("Python Process Killed.");
            }
            catch (Exception e) {
                UnityEngine.Debug.LogWarning($"SelectHandsManager: kill process failed: {e.Message}");
            }
        }

        private void StartPython() {
            string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
            string pythonExePath = System.IO.Path.Combine(projectRoot, "venv", "Scripts", "python.exe");
            string scriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "python", "main_rule_based_hands_filter.py");

            UnityEngine.Debug.Log($"[SelectHandsManager] Python Exe: {pythonExePath}");
            UnityEngine.Debug.Log($"[SelectHandsManager] Script: {scriptPath}");

            if (!System.IO.File.Exists(pythonExePath)) {
                UnityEngine.Debug.LogError($"[Error] 가상환경 Python 없음: {pythonExePath}");
                return; 
            }
            if (!System.IO.File.Exists(scriptPath)) {
                UnityEngine.Debug.LogError($"[Error] 스크립트 없음: {scriptPath}");
                return;
            }

            var psi = new ProcessStartInfo {
                FileName = pythonExePath,
                Arguments = $"-u \"{scriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try {
                _proc = new Process();
                _proc.StartInfo = psi;

                _proc.OutputDataReceived += OnPythonOutput;
                _proc.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrWhiteSpace(args.Data)) {
                        UnityEngine.Debug.LogError($"[Python Error]: {args.Data}");
                    }
                };

                _proc.Start();
                _proc.BeginOutputReadLine();
                _proc.BeginErrorReadLine();
                
                UnityEngine.Debug.Log("Python Process Started via Virtual Environment (venv).");
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError($"Python 실행 실패: {e.Message}");
            }
        }

        // [핵심 수정 부분] 문자열(LEFT, RIGHT)을 받아서 숫자(10, 20)로 변환
        private void OnPythonOutput(object sender, DataReceivedEventArgs e) {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            
            // 공백 제거 및 대문자 변환 (혹시 모를 소문자 입력 방지)
            var data = e.Data.Trim().ToUpper();

            // 로그로 들어오는 값 확인 (디버깅용)
            // UnityEngine.Debug.Log($"[Python Raw]: {data}");

            int newCode = 0;

            // 문자열에 따른 코드 매핑
            switch (data) {
                case "LEFT":
                    newCode = 10;
                    break;
                case "RIGHT":
                    newCode = 20;
                    break;
                case "BOTH": // 둘 다 들었을 때는 선택 안 함(0) 혹은 필요한 로직 추가
                    newCode = 0; 
                    break;
                case "NONE": // 손이 없으면 0
                    newCode = 0;
                    break;
                default:
                    // 이상한 값이 오면 무시하거나 0으로
                    return; 
            }

            // 값이 바뀌었을 때만 업데이트하고 로그 출력
            if (newCode != _currentHand) {
                _currentHand = newCode;
                UnityEngine.Debug.Log($"Hand Changed! Input: [{data}] -> Code: {_currentHand}");
            }
        }

        public int GetHandCode() => _currentHand;
    }
}