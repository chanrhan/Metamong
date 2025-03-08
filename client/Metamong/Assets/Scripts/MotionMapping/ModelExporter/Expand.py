import onnx
import onnx_graphsurgeon as gs
import numpy as np
from onnx import helper

# 수정할 ONNX 파일 경로
input_path = "Assets/Resources/model.onnx"
output_path = "Assets/Resources/all-MiniLM-L6-v2_fixed_input.onnx"

# ONNX 모델 로드 및 그래프 생성
model = onnx.load(input_path)
graph = gs.import_onnx(model)

# 1. 모델의 입력 "input_ids" 타입을 float32로 변경합니다.
for inp in graph.inputs:
    if inp.name == "input_ids":
        print("Changing input_ids dtype from", inp.dtype, "to float32")
        inp.dtype = np.float32
        break

# 2. 새 변수를 생성하여, "input_ids"를 int64로 캐스팅하는 Cast 노드를 삽입합니다.
new_input_ids = gs.Variable(name="input_ids_cast", dtype=np.int64)
# onnx.TensorProto.INT64는 int64의 ONNX 타입 값 (보통 7)
cast_node = gs.Node(
    op="Cast",
    name="Cast_input_ids",
    inputs=[graph.inputs[0]],  # 변경한 "input_ids" (float32)
    outputs=[new_input_ids],
    attrs={"to": onnx.TensorProto.INT64}  # 하드코딩하여 int64로 변환
)
print("Inserted Cast node to convert input_ids from float32 to int64.")

# 3. 모델 내에서 "input_ids"를 사용하는 Gather 노드들을 찾아 새 cast된 입력(new_input_ids)로 대체합니다.
for node in graph.nodes:
    if node.op == "Gather":
        for i, inp in enumerate(node.inputs):
            if inp.name == graph.inputs[0].name:
                print(f"Updating Gather node {node.name} to use cast input_ids.")
                node.inputs[i] = new_input_ids

# 4. Cast 노드를 그래프에 추가합니다.
graph.nodes.append(cast_node)

graph.cleanup()
graph.toposort()

# 수정된 모델 저장
modified_model = gs.export_onnx(graph)
onnx.save(modified_model, output_path)
print("Modified model saved to:", output_path)
