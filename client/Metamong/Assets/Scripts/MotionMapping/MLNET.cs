using System;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using UnityEngine;

class MLNET : TextEmbedding
{
    private PredictionEngine<TextData, TransformedTextData> predictionEngine;
    public MLNET(){
        predictionEngine = GetPredictionEngine();
    }
    
    public override double CompareWordText(string text1, string text2)
    {
        var word1 = new TextData{Text=text1};
        var word2 = new TextData{Text=text2};

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