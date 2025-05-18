
/*
 * 애니메이션 정보를 담는 클래스
 * 얘를 Json화 시켜서 따로 관리할 것임.
 */

[System.Serializable]
public struct MotionInfo
{
    public string[] clipNames { get; set; }     //관련된 애니메이션 클립 이름 -> 얘를 통해 애니메이션 재생
    public string emotion { get; set; }     //관련된 이모션 종류-> "기쁨", "슬픔", "놀람", "분노", "공포", "혐오", "중립"

    public MotionInfo(string[] names, string emotionText)
    {
        clipNames = names;
        emotion = emotionText;
    }
}
