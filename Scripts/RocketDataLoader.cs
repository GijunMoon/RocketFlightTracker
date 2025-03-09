using UnityEngine;
using System.Collections.Generic;

public class RocketDataLoader : MonoBehaviour
{
    public List<RocketData> rocketDataList = new List<RocketData>();

    void Awake()
    {
        // CSV 파일 로드 (Resources 폴더에서)
        TextAsset csvData = Resources.Load<TextAsset>("data");
        if (csvData != null)
        {
            rocketDataList = ParseCSV(csvData.text);
        }
        else
        {
            Debug.LogError("CSV file 'rocket_data' not found in Resources!");
        }
    }

    private List<RocketData> ParseCSV(string csvText)
    {
        var dataList = new List<RocketData>();
        var lines = csvText.Split('\n');
        for (int i = 1; i < lines.Length; i++) // 첫 번째 줄(헤더) 제외
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue; // 빈 줄 무시

            var values = line.Split(',');
            if (values.Length >= 12) // Time, Ax, Ay, Az, Gx, Gy, Gz, Angle_x, Angle_y, Angle_z, Pressure, Altitude
            {
                try
                {
                    var data = new RocketData
                    {
                        Time = float.Parse(values[0]),
                        Ax = float.Parse(values[1]),
                        Ay = float.Parse(values[2]),
                        Az = float.Parse(values[3]),
                        Gx = float.Parse(values[4]),
                        Gy = float.Parse(values[5]),
                        Gz = float.Parse(values[6]),
                        Angle_x = float.Parse(values[7]),
                        Angle_y = float.Parse(values[8]),
                        Angle_z = float.Parse(values[9]),
                        Pressure = float.Parse(values[10]),
                        Altitude = float.Parse(values[11])
                    };
                    dataList.Add(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error parsing line {i}: {line}. Exception: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid data at line {i}: {line}. Expected 12 columns, got {values.Length}");
            }
        }
        return dataList;
    }
}