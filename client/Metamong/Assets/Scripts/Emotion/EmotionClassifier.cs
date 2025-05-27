using System;
using System.Linq;
using UnityEngine;
using Unity.Sentis;

public class EmotionClassifier : MonoBehaviour
{
    private string modelResourceName = "kcElectra_emotion";
    private string vocabResourcePath = "KR_SBERT_vocab"; // Resources/Tokenizer/vocab.txt 에 위치

    // 학습 시 id2label 순서 그대로 입력
    private readonly string[] id2label = new string[]
    {
        "공포", "놀람", "분노", "슬픔", "중립", "행복", "혐오"
    };

    private Model runtimeModel;
    private Worker worker;
    private BertTokenizer tokenizer;

    void Awake()
    {
        // ONNX 모델 로드 (Resources 폴더)
        var modelAsset = Resources.Load<ModelAsset>(modelResourceName);
        if (modelAsset == null)
        {
            Debug.LogError($"Resources 폴더에서 '{modelResourceName}' 모델을 찾을 수 없습니다.");
            return;
        }
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);

        // 토크나이저 초기화 (Resources 내 vocab)
        tokenizer = new BertTokenizer(vocabResourcePath);
    }

    /// <summary>
    /// 실시간 텍스트 -> 토큰화 -> ONNX 추론 -> 레이블 반환 파이프라인
    /// </summary>
    public string Predict(string text)
    {
        // 1) 토큰 ID 및 어텐션 마스크 생성
        int[] tokenIds = tokenizer.Tokenize(text);
        int[] attentionMask = tokenizer.GetAttentionMask(tokenIds);
        int seqLen = tokenIds.Length;

        // 2) Tensor<int> 생성 및 입력 설정
        using (var idsTensor = new Tensor<int>(new TensorShape(1, seqLen), tokenIds))
        using (var maskTensor = new Tensor<int>(new TensorShape(1, seqLen), attentionMask))
        {
            worker.SetInput("input_ids", idsTensor);
            worker.SetInput("attention_mask", maskTensor);

            // 3) 모델 실행
            worker.Schedule();

            // 4) 출력 읽기 및 argmax
            var outputTensor = worker.PeekOutput() as Tensor<float>;
            outputTensor.ReadbackRequest();
            using (var result = outputTensor.ReadbackAndClone())
            {
                int labelCount = result.shape[1];
                int best = 0;
                float maxVal = result[0, 0];
                for (int i = 1; i < labelCount; i++)
                {
                    float v = result[0, i];
                    if (v > maxVal)
                    {
                        maxVal = v;
                        best = i;
                    }
                }
                return id2label[best];
            }
        }
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}