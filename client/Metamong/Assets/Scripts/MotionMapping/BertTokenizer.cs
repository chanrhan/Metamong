using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BertTokenizer
{
    private Dictionary<string, int> vocab;
    private string unkToken = "[UNK]";
    private string clsToken = "[CLS]";
    private string sepToken = "[SEP]";
    private string subwordPrefix = "##";

    /// <summary>
    /// Resources 경로(확장자 없이)를 전달하여 vocab을 로드합니다.
    /// 예: "sbert.onnx/tokenizer/vocab" (파일명이 vocab.txt라면)
    /// </summary>
    public BertTokenizer(string vocabResourcePath)
    {
        LoadVocab(vocabResourcePath);
    }

    private void LoadVocab(string vocabResourcePath)
    {
        // 확장자 없이 리소스 경로를 사용합니다.
        TextAsset vocabTextAsset = Resources.Load<TextAsset>(vocabResourcePath);
        if (vocabTextAsset == null)
        {
            Debug.LogError("Vocab file not found at: " + vocabResourcePath);
            vocab = new Dictionary<string, int>();
            return;
        }
        vocab = new Dictionary<string, int>();
        string[] lines = vocabTextAsset.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string token = lines[i].Trim();
            if (!string.IsNullOrEmpty(token))
            {
                vocab[token] = i;
            }
        }
    }

    /// <summary>
    /// 입력 텍스트를 WordPiece 토크나이저를 이용해 토큰 문자열 리스트로 반환합니다.
    /// [CLS]와 [SEP] 토큰을 자동으로 추가합니다.
    /// </summary>
    public List<string> TokenizeToTokens(string text)
    {
        List<string> tokens = new List<string>();
        tokens.Add(clsToken);
        // 텍스트를 소문자로 변환하고 공백 기준으로 단어 분할
        string[] words = text.ToLower().Split(' ');
        foreach (string word in words)
        {
            tokens.AddRange(WordPieceTokenize(word));
        }
        tokens.Add(sepToken);
        return tokens;
    }

    /// <summary>
    /// 단어 하나에 대해 WordPiece 토크나이저 알고리즘을 적용합니다.
    /// 가능한 가장 긴 서브워드를 찾고, 시작이 아닌 경우 "##" 접두사를 붙입니다.
    /// 매칭 실패 시 [UNK] 토큰을 반환합니다.
    /// </summary>
    private List<string> WordPieceTokenize(string word)
    {
        List<string> subTokens = new List<string>();
        int start = 0;
        bool isBad = false;

        while (start < word.Length)
        {
            int end = word.Length;
            string curSubstr = null;
            while (start < end)
            {
                string substr = word.Substring(start, end - start);
                if (start > 0)
                {
                    substr = subwordPrefix + substr;
                }
                if (vocab.ContainsKey(substr))
                {
                    curSubstr = substr;
                    break;
                }
                end--;
            }
            if (curSubstr == null)
            {
                isBad = true;
                break;
            }
            subTokens.Add(curSubstr);
            start = end;
        }
        if (isBad)
        {
            return new List<string> { unkToken };
        }
        return subTokens;
    }

    /// <summary>
    /// 입력 텍스트를 토큰화한 후, 각 토큰을 vocab에서 ID로 매핑하여 정수 배열을 반환합니다.
    /// </summary>
    public int[] Tokenize(string text)
    {
        List<string> tokens = TokenizeToTokens(text);
        List<int> tokenIds = new List<int>();
        foreach (var token in tokens)
        {
            if (vocab.ContainsKey(token))
            {
                tokenIds.Add(vocab[token]);
            }
            else
            {
                tokenIds.Add(vocab.ContainsKey(unkToken) ? vocab[unkToken] : 0);
            }
        }
        return tokenIds.ToArray();
    }

    /// <summary>
    /// 입력된 토큰 ID 배열과 동일한 길이의 어텐션 마스크(모두 1)를 반환합니다.
    /// </summary>
    public int[] GetAttentionMask(int[] tokenIds)
    {
        return tokenIds.Select(id => 1).ToArray();
    }
}
