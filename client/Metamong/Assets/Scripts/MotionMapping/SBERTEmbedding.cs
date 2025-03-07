using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Sentis;           // Sentis 관련 API

public class SBERTEmbedding : TextEmbedding
{
    private Model runtimeModel;
    private Worker worker;
    private BertTokenizer tokenizer;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Resources 폴더에서 "model" 에셋을 로드 (확장자 없이)
        ModelAsset asset = Resources.Load<ModelAsset>("model");
        if (asset == null)
        {
            Debug.LogError("Resources 폴더에서 'model' 에셋을 찾을 수 없습니다.");
            return;
        }
        runtimeModel = ModelLoader.Load(asset);

        // Sentis Worker 생성 (GPUCompute 백엔드 사용)
        worker = new Worker(runtimeModel, BackendType.CPU);

        // Resources 폴더 내의 vocab 파일 경로 (확장자 없이)
        tokenizer = new BertTokenizer("sbert.onnx/vocab");
    }

    public override double CompareWordText(string text1, string text2)
    {
        float[] emb1 = GetEmbedding(text1);
        float[] emb2 = GetEmbedding(text2);
        return CosineSimilarity(emb1, emb2);
    }

    public float[] GetEmbedding(string text)
    {
        // 토크나이저를 통해 토큰 ID 배열 생성
        int[] tokenIds = tokenizer.Tokenize(text);
        int[] attentionMask = tokenizer.GetAttentionMask(tokenIds);
        int length = tokenIds.Length;

        // "input_ids" 텐서 생성 (2D 텐서: [1, length])
        Tensor<float> inputIdsTensor = new Tensor<float>(new TensorShape(1, length),
            tokenIds.Select(id => (float)id).ToArray());
        Debug.Log("Input IDs Tensor Shape: " + inputIdsTensor.shape.ToString());

        // "attention_mask" 텐서 생성 (2D 텐서: [1, length])
        Tensor<float> attentionMaskTensor = new Tensor<float>(new TensorShape(1, length),
            attentionMask.Select(val => (float)val).ToArray());
        Debug.Log("Attention Mask Tensor Shape: " + attentionMaskTensor.shape.ToString());

        // 각 입력 텐서를 개별적으로 설정합니다.
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);
        worker.Schedule();

        // 출력 텐서 획득; 출력 이름은 모델 메타데이터에 따라 결정됩니다.
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        Debug.Log("Output Tensor Shape: " + outputTensor.shape.ToString());

        // TensorShape의 rank 속성을 사용하여 출력 텐서의 차원 수를 확인합니다.
        int rank = outputTensor.shape.rank;
        Debug.Log("Output Tensor Rank: " + rank);

        float[] embedding;
        if (rank == 3)
        {
            int hiddenSize = outputTensor.shape[2];
            embedding = new float[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                embedding[i] = outputTensor[0, 0, i];
            }
        }
        else if (rank == 2)
        {
            int hiddenSize = outputTensor.shape[1];
            embedding = new float[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                embedding[i] = outputTensor[0, i];
            }
        }
        else
        {
            Debug.LogError("Unexpected output tensor rank: " + rank);
            embedding = new float[0];
        }

        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
        outputTensor.Dispose();

        return embedding;
    }
    private void OnDestroy()
    {
        worker?.Dispose();
    }
}