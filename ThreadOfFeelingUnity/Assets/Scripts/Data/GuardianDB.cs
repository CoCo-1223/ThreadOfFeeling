using UnityEngine;
using System;
using SQLite;
/// <summary>
///  2. Guardian - 보호자 계정 및 동의 정보 
/// </summary>
[Table("Guardian")]
public class GuardianDB {

    // guardian_id, INTEGER, 보호자 ID
    [PrimaryKey, AutoIncrement]
    public int guardian_id { get; set; }

    // pin_hash, hash (TEXT로 저장)
    public string pin_hash { get; set; }

    // consent_ts, DATETIME, 정보 수집 동의한 시간
    public DateTime consent_ts { get; set; }
    
    // children[] (ChildProfile[])는 C# 코드(로직) 레벨에서 처리
}