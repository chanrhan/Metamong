using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Sentis; // Sentis 관련 API

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
        Tensor<float> inputIdsTensor = new Tensor<float>(
            new TensorShape(1, length),
            tokenIds.Select(id => (float)id).ToArray());
        Debug.Log("Input IDs Tensor Shape: " + inputIdsTensor.shape.ToString());

        // "attention_mask" 텐서 생성 (2D 텐서: [1, length])
        Tensor<float> attentionMaskTensor = new Tensor<float>(
            new TensorShape(1, length),
            attentionMask.Select(val => (float)val).ToArray());
        Debug.Log("Attention Mask Tensor Shape: " + attentionMaskTensor.shape.ToString());

        // 각 입력 텐서를 설정하고 모델 실행 시작
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);
        worker.Schedule();

        // 출력 텐서를 PeekOutput으로 참조 (worker가 소유하므로 Dispose 불필요)
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        Debug.Log("Output Tensor Shape (PeekOutput): " + outputTensor.shape.ToString());

        // 출력 텐서의 데이터가 준비될 때까지 비동기 readback 요청 (blocking 방식)
        outputTensor.ReadbackRequest();
        Tensor<float> clonedOutput = outputTensor.ReadbackAndClone();
        Debug.Log("Cloned Output Tensor Shape: " + clonedOutput.shape.ToString());

        // TensorShape의 rank 속성을 사용하여 출력 텐서의 차원 수를 확인합니다.
        int rank = clonedOutput.shape.rank;
        Debug.Log("Cloned Output Tensor Rank: " + rank);

        float[] embedding;
        if (rank == 3)
        {
            int hiddenSize = clonedOutput.shape[2];
            embedding = new float[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                embedding[i] = clonedOutput[0, 0, i];
            }
        }
        else if (rank == 2)
        {
            int hiddenSize = clonedOutput.shape[1];
            embedding = new float[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                embedding[i] = clonedOutput[0, i];
            }
        }
        else
        {
            Debug.LogError("Unexpected output tensor rank: " + rank);
            embedding = new float[0];
        }

        // 입력 텐서는 사용 후 Dispose합니다.
        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
        // clonedOutput는 우리가 복사한 텐서이므로 Dispose하여 메모리를 해제합니다.
        clonedOutput.Dispose();

        return embedding;
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}
