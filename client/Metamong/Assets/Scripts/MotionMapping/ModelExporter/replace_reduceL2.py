import torch
import torch.nn.functional as F
import math
from transformers import AutoModel, AutoTokenizer
from onnxsim import simplify
import onnx

# 커스텀 scaled_dot_product_attention 함수 정의 및 monkey-patch (이전과 동일)
def custom_scaled_dot_product_attention(q, k, v, attn_mask=None, dropout_p=0.0, is_causal=False):
    d_k = q.size(-1)
    attn_scores = torch.matmul(q, k.transpose(-2, -1)) / math.sqrt(d_k)
    if attn_mask is not None:
        attn_scores = attn_scores + attn_mask
    attn_probs = torch.softmax(attn_scores, dim=-1)
    if dropout_p > 0.0:
        attn_probs = F.dropout(attn_probs, p=dropout_p)
    output = torch.matmul(attn_probs, v)
    return output

torch.nn.functional.scaled_dot_product_attention = custom_scaled_dot_product_attention

# 모델 및 토크나이저 로드
model_name = "sentence-transformers/all-MiniLM-L6-v2"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModel.from_pretrained(model_name)
model.eval()

# 예시 입력 생성 (최대 길이 128)
text = "This is a sample sentence for ONNX export."
inputs = tokenizer(text, return_tensors="pt", padding="max_length", max_length=128, truncation=True)
# Unity에서는 float 텐서를 전달하므로, dummy_input도 float로 생성합니다.
dummy_input = (inputs["input_ids"].float(), inputs["attention_mask"].float())

# ModelWrapper: 입력 텐서를 내부에서 정수형으로 캐스팅하고, 평균 풀링을 수행
class ModelWrapper(torch.nn.Module):
    def __init__(self, model):
        super(ModelWrapper, self).__init__()
        self.model = model
    def forward(self, input_ids, attention_mask):
        # Unity에서 전달된 float 텐서를 정수형으로 캐스팅
        input_ids = input_ids.long()
        attention_mask = attention_mask.long()
        outputs = self.model(input_ids=input_ids, attention_mask=attention_mask)
        token_embeddings = outputs.last_hidden_state  # [batch, seq, hidden]
        # 마스크 텐서의 shape: [batch, seq] -> unsqueeze하여 [batch, seq, 1]
        mask = attention_mask.unsqueeze(-1).float()
        # 곱셈에서 자동 브로드캐스트가 적용됨 (Expand 노드가 생성되지 않음)
        sum_embeddings = torch.sum(token_embeddings * mask, dim=1)
        # 마스크 합이 0이 되는 경우를 방지하기 위해 클램프
        sum_mask = torch.clamp(mask.sum(dim=1), min=1e-9)
        pooled = sum_embeddings / sum_mask
        return pooled

wrapped_model = ModelWrapper(model)

# ONNX로 내보내기 (opset_version=14, 동적 연산 유지)
export_path = "C:/git/Metamong/client/Metamong/Assets/Resources/all-MiniLM-L6-v2.onnx"
torch.onnx.export(
    wrapped_model,
    dummy_input,
    export_path,
    export_params=True,
    opset_version=14,
    do_constant_folding=False,
    input_names=['input_ids', 'attention_mask'],
    output_names=['pooled_embedding'],
    dynamic_axes={
        'input_ids': {0: 'batch', 1: 'seq'},
        'attention_mask': {0: 'batch', 1: 'seq'},
        'pooled_embedding': {0: 'batch'}
    }
)

print("Model exported to", export_path)

# ONNX Simplifier를 적용하여 모델 단순화 (옵션)
model_onnx = onnx.load(export_path)
model_simp, check = simplify(model_onnx)
assert check, "Simplified ONNX model could not be validated"
simplified_export_path = "C:/git/Metamong/client/Metamong/Assets/Resources/all-MiniLM-L6-v2_simplified.onnx"
onnx.save(model_simp, simplified_export_path)
print("Simplified model saved to", simplified_export_path)
