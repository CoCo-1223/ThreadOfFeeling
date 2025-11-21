using System;
using System.Diagnostics;
using UnityEngine;

namespace Managers {
    public class SelectHandsManager : MonoBehaviour {
        public static SelectHandsManager Instance { get; private set; }
        private Process _proc;
        private int _currentHand = 0;

        private void Awake() {
            // 싱글톤 패턴은 유지하되, 씬 이동 시 파괴되도록 DontDestroyOnLoad는 삭제합니다.
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            StartPython();
        }

        private void OnDestroy() {
            // 씬이 바뀌거나 게임이 꺼질 때 파이썬 종료
            KillPythonProcess();
            
            // 인스턴스 참조 해제 (중요: 다음 씬에서 null 체크가 올바르게 동작하도록)
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
            string pythonScriptPath = "../python/main_rule_based_hands_filter.py"; 

            var psi = new ProcessStartInfo {
                FileName = "python", // 혹은 "python3" (설치된 환경에 따라 다름)
                // [중요 1] -u 옵션: 버퍼링 없이 즉시 출력
                Arguments = $"-u \"{pythonScriptPath}\"", 
                RedirectStandardOutput = true,
        
                // [중요 2] 이 줄이 빠져서 에러가 난 것입니다. 꼭 추가해주세요!
                RedirectStandardError = true, 
        
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try {
                _proc = new Process();
                _proc.StartInfo = psi;

                // 1. 정상 출력 수신 (Hand Code 등)
                _proc.OutputDataReceived += OnPythonOutput;
        
                // 2. 에러 로그 수신 (파이썬 내부 오류 확인용)
                _proc.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrWhiteSpace(args.Data)) {
                        // 빨간색 로그로 파이썬 에러를 띄워줍니다.
                        UnityEngine.Debug.LogError($"[Python Error]: {args.Data}");
                    }
                };

                _proc.Start();
        
                // 스트림 읽기 시작
                _proc.BeginOutputReadLine();
                _proc.BeginErrorReadLine(); // [중요 2]와 짝꿍입니다.
        
                UnityEngine.Debug.Log("Python Process Started for Story Scene.");
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError($"Python 실행 실패: {e.Message}");
            }
        }   

        private void OnPythonOutput(object sender, DataReceivedEventArgs e) {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            var raw = e.Data.Trim();

            if (!int.TryParse(raw, out var code)) return;

            if (code == 10 || code == 20) {
                if (code != _currentHand) {
                    _currentHand = code;
                }
            }
        }

        public int GetHandCode() => _currentHand;
    }
}