using System;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using UnityEngine;

/// <summary>
/// <para>
/// 텍스트 임베딩 모델인 ML_NET 을 사용하여 임베딩을 수행하는 클래스
/// </para>
/// 
/// </summary>
class MLNET : TextEmbedding
{
    private PredictionEngine<TextData, TransformedTextData> predictionEngine;
    public MLNET(){
        predictionEngine = GetPredictionEngine(); 
    }
    
    public override double CompareWordText(string text1, string text2)
    {
        // 문자열을 TextData 객체에 담아서 Predict 함수에 넣어야 됨 
        var word1 = new TextData{Text=text1};
        var word2 = new TextData{Text=text2};

        // 문자열을 임베딩 벡터로 변환 
        var prediction1 = predictionEngine.Predict(word1);
        var prediction2 = predictionEngine.Predict(word2);

        return CosineSimilarity(prediction1.Features, prediction2.Features);
    }

    public class TextData
    {
        public string Text { get; set; }
    }

    public class TransformedTextData : TextData
    {
        public float[] Features { get; set; }
    }
    
    /// <summary>
    /// 문자열을 임베딩해주는 예측 엔진을 생성
    /// <para>
    /// <returns></returns>
    public static PredictionEngine<TextData, TransformedTextData> GetPredictionEngine()
    {
        Debug.Log("Apply Word Embedding Begin...");
        // Create a new ML context, for ML.NET operations. It can be used for
        // exception tracking and logging, as well as the source of randomness.
        var mlContext = new MLContext();

        // Create an empty list as the dataset. The 'ApplyWordEmbedding' does
        // not require training data as the estimator ('WordEmbeddingEstimator')
        // created by 'ApplyWordEmbedding' API is not a trainable estimator.
        // The empty list is only needed to pass input schema to the pipeline.
        // 
        // (번역 by GPT) ApplyWordEmbedding은 훈련 데이터를 필요로 하지 않는데, 
        // 이는 ApplyWordEmbedding API가 생성하는 WordEmbeddingEstimator가 훈련 가능한 추정기(estimator)가 아니기 때문입니다.
        // 빈 리스트는 단순히 파이프라인에 입력 스키마를 전달하기 위해 필요합니다.
        var emptySamples = new List<TextData>();

        // Convert sample list to an empty IDataView.
        var emptyDataView = mlContext.Data.LoadFromEnumerable(emptySamples);

        // A pipeline for converting text into a 150-dimension embedding vector
        // using pretrained 'SentimentSpecificWordEmbedding' model. The
        // 'ApplyWordEmbedding' computes the minimum, average and maximum values
        // for each token's embedding vector. Tokens in 
        // 'SentimentSpecificWordEmbedding' model are represented as
        // 50 -dimension vector. Therefore, the output is of 150-dimension [min,
        // avg, max].
        //
        // The 'ApplyWordEmbedding' API requires vector of text as input.
        // The pipeline first normalizes and tokenizes text then applies word
        // embedding transformation.
        // 
        // (번역 by GPT) 사전 학습된 SentimentSpecificWordEmbedding 모델을 사용하여 텍스트를 150차원 임베딩 벡터로 변환하는 파이프라인입니다.
        // ApplyWordEmbedding은 각 토큰의 임베딩 벡터에 대해 최소값, 평균값, 최대값을 계산합니다.
        // SentimentSpecificWordEmbedding 모델에서 각 토큰은 50차원 벡터로 표현되므로, 출력 벡터는 [최소값, 평균값, 최대값]을 포함한 150차원 벡터가 됩니다.

        // ApplyWordEmbedding API는 텍스트의 벡터 입력을 요구합니다.
        // 따라서 파이프라인은 먼저 텍스트를 정규화하고 토큰화한 후, 단어 임베딩 변환을 적용합니다.
        var textPipeline = mlContext.Transforms.Text.NormalizeText("Text")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens",
                "Text"))
            .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features",
                "Tokens", WordEmbeddingEstimator.PretrainedModelKind
                .SentimentSpecificWordEmbedding));

        // Fit to data.
        var textTransformer = textPipeline.Fit(emptyDataView);

        // Create the prediction engine to get the embedding vector from the
        // input text/string.
        var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
            TransformedTextData>(textTransformer);

        return predictionEngine;
    }

}