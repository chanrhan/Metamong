using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Word2vec;
using Word2vec.Tools;

public class Word2VecModel : ITextEmbeddable
{
    private int vectorSize = 10;
    public Word2VecModel(int vectorSize){
        this.vectorSize = vectorSize;
    }

    public double GetCosinSimilarity(string org, string target)
    {
        float[] targetVectors = UseWord2VecFromTensorflow(org)[0];
        float[] comparedVectors = UseWord2VecFromTensorflow(target)[0];
        double cos = GetCosineSimilarityTest(targetVectors, comparedVectors);
        return cos;
    }

    void UseWord2VecTools(){
        
    }

    void UseWord2VecFromMicrosoftSpark(){
        // var word2vec = new Word2Vec();
    }

    List<float[]> UseWord2VecFromTensorflow(string word){
        // TensorFlow 모델 설정
        // var session = new Session();

        // 예시 텍스트 데이터

        // 텍스트 데이터를 처리하여 토큰화
        // var tokenizedSentences = TokenizeSentences(sentences);
        var tokens = word.Split(' ');
        // 단어를 벡터로 변환 (여기서는 임시로 무작위 값 사용)
        var wordVectors = GenerateRandomWordVectors(tokens.Length, vectorSize);

        // Word2Vec 모델을 훈련시키는 부분은 TensorFlow로 학습을 진행해야 합니다.
        // 학습 루프 및 모델 훈련은 TensorFlow 모델과 학습 데이터를 구성하여 진행됩니다.
        
        // 간단한 출력 예시
        // Debug.Log("Word Vectors:");
        // for (int i = 0; i < tokens.Length; i++)
        // {
        //     Debug.Log($"{tokens[i]}: {string.Join(", ", wordVectors[i])}");
        // }


        return wordVectors;
    }

    // 텍스트 데이터를 단어별로 분할하는 함수
    static List<List<string>> TokenizeSentences(List<string> sentences)
    {
        var tokenized = new List<List<string>>();
        foreach (var sentence in sentences)
        {
            var tokens = sentence.Split(' ');
            tokenized.Add(new List<string>(tokens));
        }
        return tokenized;
    }

    static List<float[]> GenerateRandomWordVectors(int numWords, int vectorSize)
    {
        var random = new System.Random();
        var vectors = new List<float[]>();
        for (int i = 0; i < numWords; i++)
        {
            var vector = new float[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                vector[j] = (float)(random.NextDouble() - 0.5); // -0.5 to 0.5 범위의 값
            }
            vectors.Add(vector);
        }
        return vectors;
    }

    double GetCosineSimilarityTest(float[] a,float[] b)
    {
        double magA2 = 0;
        double magB2 = 0;
        double product = 0;

        for (int i=0;i<3;i++)
        {
            magA2 += Math.Pow(a[i], 2);
            magB2 += Math.Pow(b[i], 2);
            product += (a[i] * 1.0 * b[i]);
        }

        double magAB = Math.Sqrt(magA2 * magB2);
        return product / magAB;
    }
}