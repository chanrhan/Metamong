using System;

/// <summary>
/// 텍스트 임베딩 모델을 모듈화하기 위해 만든 추상 클래스
/// 다른 임베딩 모델을 사용할 것이라면, 해당 추상 클래스를 상속받아 만들면 된다 (ex. MLNET.cs)
/// </summary>
public abstract class TextEmbedding{
    /// <summary>
    /// <para>
    /// 문자열 text1, text2의 유사도를 비교
    /// </para>
    /// 비교하는 로직은 상속하는 임베딩 모델 객체에서 overridding 해서 구현하기 바람 
    /// </summary>
    /// <returns>문자열 text1, text2 사이의 유사도 수치 (0.00 ~ 1.00)</returns>
    public abstract double CompareWordText(string text1, string text2);

    /// <summary>
    /// 두 임베딩 벡터의 코사인 유사도 검사를 수행
    /// </summary>
    /// <returns>두 임베딩 벡터 사이의 코사인 유사도 수치</returns>
    protected double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        double dotProduct = 0, normA = 0, normB = 0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}