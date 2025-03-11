using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Sentis; // Sentis 관련 API

public class SBERTEmbedding : MonoBehaviour
{
    private Model runtimeModel;
    private Worker worker;
    private BertTokenizer tokenizer;

    private float threshold = 0.6f;

    // 일반 텍스트에 대한 임베딩 캐시
    private Dictionary<string, float[]> embeddingCache = new Dictionary<string, float[]>();

    // 미리 임베딩할 기존 모션 문장들 (문장 -> 모션 키)
    public Dictionary<string, string> actMotionList = new Dictionary<string, string>();
    public Dictionary<string, string> faceMotionList = new Dictionary<string, string>();

    // 미리 계산된 임베딩 벡터 캐시 (문장 -> 임베딩 벡터)
    public Dictionary<string, float[]> actMotionEmbeddings = new Dictionary<string, float[]>();
    public Dictionary<string, float[]> faceMotionEmbeddings = new Dictionary<string, float[]>();

    // JSON 파일 경로 (절대 경로 예: Application.dataPath 기준)
    private static string ACTMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/ActMotion.json";
    private static string FACEMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/FaceMotion.json";


    void Awake()
    {
        Initialize();
        PrecomputeMotionEmbeddings();
    }

    /// <summary>
    /// 모델을 로드 및 초기화
    /// </summary>
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

        // Sentis Worker 생성 (예제에서는 CPU 백엔드를 사용)
        worker = new Worker(runtimeModel, BackendType.CPU);

        // Resources 폴더 내의 vocab 파일 경로 (확장자 없이)
        tokenizer = new BertTokenizer("sbert.onnx/vocab");
    }

    /// <summary>
    /// 앱 시작 시 기존 모션 문장들을 JSON 파일에서 읽어와 미리 임베딩 벡터를 계산해 저장합니다.
    /// JsonFileReader 클래스는 파일을 읽어 Dictionary&lt;string, string&gt;를 반환합니다.
    /// </summary>
    public void PrecomputeMotionEmbeddings()
    {
        // ActMotion, FaceMotion 파일을 읽어옵니다.
        actMotionList = JsonFileReader.Read(ACTMOTION_FILE_PATH);
        faceMotionList = JsonFileReader.Read(FACEMOTION_FILE_PATH);

        // 각 ActMotion 문장에 대해 임베딩을 계산하여 캐시에 저장합니다.
        foreach (var kvp in actMotionList)
        {
            string sentence = kvp.Key;
            if (!actMotionEmbeddings.ContainsKey(sentence))
            {
                actMotionEmbeddings[sentence] = GetEmbedding(sentence);
            }
        }
        Debug.Log("Precomputed ActMotion embeddings: " + actMotionEmbeddings.Count);

        // 각 FaceMotion 문장에 대해 임베딩을 계산하여 캐시에 저장합니다.
        foreach (var kvp in faceMotionList)
        {
            string sentence = kvp.Key;
            if (!faceMotionEmbeddings.ContainsKey(sentence))
            {
                faceMotionEmbeddings[sentence] = GetEmbedding(sentence);
            }
        }
        Debug.Log("Precomputed FaceMotion embeddings: " + faceMotionEmbeddings.Count);
    }

    /// <summary>
    /// 두 텍스트 간의 코사인 유사도를 계산합니다.
    /// (입력 텍스트는 실시간으로 임베딩 벡터로 변환되고, 기존 문장들과 비교할 수 있습니다.)
    /// </summary>
    public string CompareWordText(string inputText, bool isAct)
    {
        if (inputText == "none")
        {
            return "No match";
        }

        float[] inputEmbedding = GetEmbedding(inputText);

        // 카테고리에 따라 미리 계산된 임베딩 딕셔너리를 선택합니다.
        Dictionary<string, float[]> precomputedEmbeddings = isAct ? actMotionEmbeddings : faceMotionEmbeddings;

        double bestScore = -1.0;
        string bestMatchSentence = string.Empty;
        string bestMatchKey = string.Empty;

        // 미리 계산된 각 문장에 대해 코사인 유사도를 계산합니다.
        foreach (KeyValuePair<string, float[]> kvp in precomputedEmbeddings)
        {
            double score = CosineSimilarity(inputEmbedding, kvp.Value);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatchSentence = kvp.Key;
                bestMatchKey = isAct ? actMotionList[kvp.Key] : faceMotionList[kvp.Key];
            }
        }
        Debug.Log($"Best match Sentence :{inputText} => {bestMatchSentence} ({bestScore})");
        return bestScore >= threshold ? bestMatchKey : "No match";
    }


    /// <summary>
    /// 텍스트를 모델을 통해 임베딩 벡터로 변환합니다.
    /// 동일한 텍스트에 대해서는 캐싱하여 중복 계산을 피합니다.
    /// </summary>
    /// <param name="text">임베딩할 텍스트</param>
    /// <returns>임베딩된 벡터</returns>
    public float[] GetEmbedding(string text)
    {
        // 캐시에 이미 임베딩 결과가 있다면 반환합니다.
        if (embeddingCache.ContainsKey(text))
        {
            return embeddingCache[text];
        }

        // 토크나이저를 통해 토큰 ID 및 attention mask 배열 생성
        int[] tokenIds = tokenizer.Tokenize(text);
        int[] attentionMask = tokenizer.GetAttentionMask(tokenIds);
        int length = tokenIds.Length;

        // "input_ids" 텐서 생성 (배치 크기 1, 길이: tokenIds.Length)
        Tensor<float> inputIdsTensor = new Tensor<float>(
            new TensorShape(1, length),
            tokenIds.Select(id => (float)id).ToArray());

        // "attention_mask" 텐서 생성 (배치 크기 1)
        Tensor<float> attentionMaskTensor = new Tensor<float>(
            new TensorShape(1, length),
            attentionMask.Select(val => (float)val).ToArray());

        // 텐서를 입력으로 설정하고 모델 실행
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);
        worker.Schedule();

        // 출력 텐서를 PeekOutput()으로 참조합니다. (worker가 소유하므로 Dispose 불필요)
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        // 출력 텐서의 데이터가 준비될 때까지, Readback 요청 후 클론 텐서를 blocking 방식으로 가져옵니다.
        outputTensor.ReadbackRequest();
        Tensor<float> clonedOutput = outputTensor.ReadbackAndClone();

        int hiddenSize = clonedOutput.shape[1];
        float[] embedding = new float[hiddenSize];
        for (int i = 0; i < hiddenSize; i++)
        {
            embedding[i] = clonedOutput[0, i];
        }

        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
        clonedOutput.Dispose();

        // 결과를 캐시에 저장
        embeddingCache[text] = embedding;

        return embedding;
    }
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

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}
