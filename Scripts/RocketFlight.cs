using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RocketFlight : MonoBehaviour
{
    public RocketDataLoader dataLoader;
    public RocketTrail rocketTrail;
    public Transform rocketModel;

    private List<RocketData> rocketDataList;
    private List<Vector3> positions;
    private List<Quaternion> orientations;
    private float startTime;

    public float positionScale = 1f; // 미터 단위를 Unity 유닛으로 직접 사용
    public bool mapAzToY = false; // Az를 Y축으로 매핑하지 않음, Altitude 사용

    void Start()
    {
        if (dataLoader != null)
        {
            rocketDataList = dataLoader.rocketDataList;
            if (rocketDataList == null || rocketDataList.Count == 0)
            {
                Debug.LogError("rocketDataList is empty or not initialized!");
                return;
            }

            CalculatePositionsAndOrientations();
            startTime = Time.time;

            if (positions.Count > 0)
            {
                rocketModel.transform.position = positions[0];
                rocketModel.transform.rotation = orientations[0];
                rocketTrail.UpdateLineFirst(positions[0]);
            }

            Debug.Log("Position Range: " + positions.Min(v => v.magnitude) + " to " + positions.Max(v => v.magnitude));
            if (rocketDataList.Count > 0)
            {
                Debug.Log("Max Time: " + rocketDataList[rocketDataList.Count - 1].Time * 1E-6f + " seconds");
                Debug.Log($"rocketDataList Count: {rocketDataList.Count}, positions Count: {positions.Count}, orientations Count: {orientations.Count}");
            }
        }
        else
        {
            Debug.LogError("DataLoader not assigned in RocketFlight!");
        }

        if (rocketModel == null)
        {
            Debug.LogError("rocketModel not assigned in RocketFlight!");
        }

        if (rocketTrail == null)
        {
            Debug.LogError("rocketTrail not assigned in RocketFlight!");
        }
    }

    void CalculatePositionsAndOrientations()
    {
        positions = new List<Vector3>();
        orientations = new List<Quaternion>();

        Vector3 position = Vector3.zero;
        Vector3 velocity = Vector3.zero; // 초기 속도 제거, 데이터에 의존
        Quaternion orientation = Quaternion.Euler(0f, 0f, 0f); // 초기 회전 초기화
        float previousTime = rocketDataList[0].Time * 1E-6f; // 첫 번째 시간으로 초기화

        // 첫 번째 데이터 포인트 추가
        positions.Add(position);
        orientations.Add(orientation);

        for (int i = 1; i < rocketDataList.Count; i++)
        {
            var data = rocketDataList[i];
            float currentTime = data.Time * 1E-6f; // 마이크로세컨드 → 초
            float deltaTime = currentTime - previousTime;
            if (deltaTime >= 0) // deltaTime이 음수일 경우도 처리
            {
                Debug.Log($"Time: {currentTime}, Ax: {data.Ax}, Ay: {data.Ay}, Az: {data.Az}, Altitude: {data.Altitude}");

                // 가속도 기반 이동 (X, Z만 사용, Y는 Altitude로 대체)
                Vector3 acceleration = new Vector3(data.Ax, 0f, data.Az); // Ay는 무시, Y는 Altitude로
                velocity += acceleration * deltaTime;
                position.x += velocity.x * deltaTime;
                position.z += velocity.z * deltaTime;

                // Y 위치는 Altitude로 직접 설정
                position.y = data.Altitude * positionScale;

                positions.Add(position);

                // 회전 계산 (Angle_x, Angle_y, Angle_z 사용)
                Vector3 angles = new Vector3(data.Angle_x, data.Angle_y, data.Angle_z);
                orientation = Quaternion.Euler(angles);
                orientations.Add(orientation);

                previousTime = currentTime;
            }
        }
    }

    void Update()
    {
        if (rocketDataList != null && positions != null && orientations != null && rocketModel != null && rocketTrail != null && positions.Count == rocketDataList.Count && orientations.Count == rocketDataList.Count)
        {
            float elapsedTime = Time.time - startTime; // Unity의 경과 시간
            float maxTime = rocketDataList[rocketDataList.Count - 1].Time * 1E-6f; // 데이터의 최대 시간

            // 데이터 시간에 맞는 인덱스 계산
            int index = 0;
            while (index < rocketDataList.Count - 1 && elapsedTime >= rocketDataList[index + 1].Time * 1E-6f)
            {
                index++;
            }
            index = Mathf.Clamp(index, 0, positions.Count - 1); // 인덱스 범위 제한

            if (elapsedTime <= maxTime)
            {
                float segmentStartTime = index > 0 ? rocketDataList[index].Time * 1E-6f : 0f;
                float segmentEndTime = index < rocketDataList.Count - 1 ? rocketDataList[index + 1].Time * 1E-6f : maxTime;
                float t = Mathf.Clamp01((elapsedTime - segmentStartTime) / (segmentEndTime - segmentStartTime));

                Vector3 pos = Vector3.Lerp(positions[index], positions[Mathf.Min(index + 1, positions.Count - 1)], t);
                Quaternion rot = Quaternion.Slerp(orientations[index], orientations[Mathf.Min(index + 1, orientations.Count - 1)], t);

                rocketModel.transform.position = pos;
                rocketModel.transform.rotation = rot;
                rocketTrail.UpdateLineFirst(pos);
            }
            else
            {
                // 최대 시간 초과 시 마지막 위치 유지
                Vector3 finalPos = positions[positions.Count - 1];
                Quaternion finalRot = orientations[orientations.Count - 1];

                rocketModel.transform.position = finalPos;
                rocketModel.transform.rotation = finalRot;
                rocketTrail.UpdateLineLast(positions, positions.Count - 1);
            }
        }
    }
}