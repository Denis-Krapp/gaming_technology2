import os
import torch
import torch.nn as nn
import onnx
from onnx import helper, TensorProto
import numpy as np

checkpoint_path = "results/sparse_v1/RollerAgent/RollerAgent-55496.pt"
output_path = "GamingTech_Practice1/Assets/Models/sparse/RollerAgent.onnx"

os.makedirs(os.path.dirname(output_path), exist_ok=True)

# 1. Network definition
class RollerAgentPolicy(nn.Module):
    def __init__(self, state_size=8, action_size=2, hidden_units=128):
        super(RollerAgentPolicy, self).__init__()
        self.net = nn.Sequential(
            nn.Linear(state_size, hidden_units),
            nn.Tanh(),
            nn.Linear(hidden_units, hidden_units),
            nn.Tanh(),
            nn.Linear(hidden_units, action_size)
        )

    def forward(self, obs_0):
        return self.net(obs_0)

# 2. Load weights
print(f"Loading weights from: {checkpoint_path}")
checkpoint = torch.load(checkpoint_path, map_location="cpu")
raw_state = checkpoint.get("Policy", checkpoint)
if hasattr(raw_state, "state_dict"):
    raw_state = raw_state.state_dict()

linear_weights = []
linear_biases = []
for key in sorted(raw_state.keys()):
    if "weight" in key and "linear" in key.lower():
        linear_weights.append(raw_state[key])
    elif "bias" in key and "linear" in key.lower():
        linear_biases.append(raw_state[key])

model = RollerAgentPolicy(state_size=8, action_size=2, hidden_units=128)

if len(linear_weights) >= 3 and len(linear_biases) >= 3:
    mapped_state = {
        "net.0.weight": linear_weights[0],
        "net.0.bias": linear_biases[0],
        "net.2.weight": linear_weights[1],
        "net.2.bias": linear_biases[1],
        "net.4.weight": linear_weights[2],
        "net.4.bias": linear_biases[2]
    }
    model.load_state_dict(mapped_state, strict=False)
    print("Successfully mapped linear layers.")
else:
    model.load_state_dict(raw_state, strict=False)

model.eval()

# 3. Export base model with opset 16 (Sentis native standard)
dummy_input = torch.randn(1, 8, dtype=torch.float32)

torch.onnx.export(
    model,
    dummy_input,
    output_path,
    export_params=True,
    opset_version=16,
    input_names=["obs_0"],
    output_names=["continuous_actions"],
    dynamic_axes={
        "obs_0": {0: "batch_size"},
        "continuous_actions": {0: "batch_size"}
    }
)

# 4. Inject Constant nodes into graph
model_proto = onnx.load(output_path)
graph = model_proto.graph

# Clean previous conflicting entries if any
names_to_clean = ["version_number", "memory_size", "continuous_action_output_shape"]
for i in range(len(graph.initializer) - 1, -1, -1):
    if graph.initializer[i].name in names_to_clean:
        del graph.initializer[i]
for i in range(len(graph.node) - 1, -1, -1):
    if any(out in names_to_clean for out in graph.node[i].output):
        del graph.node[i]

# Add Constant nodes directly as graph operations
node_version = helper.make_node(
    "Constant",
    inputs=[],
    outputs=["version_number"],
    value=helper.make_tensor("v_val", TensorProto.INT64, [], [3])
)

node_mem = helper.make_node(
    "Constant",
    inputs=[],
    outputs=["memory_size"],
    value=helper.make_tensor("m_val", TensorProto.INT64, [], [0])
)

node_act = helper.make_node(
    "Constant",
    inputs=[],
    outputs=["continuous_action_output_shape"],
    value=helper.make_tensor("a_val", TensorProto.INT64, [1], [2])
)

graph.node.extend([node_version, node_mem, node_act])

onnx.save(model_proto, output_path)
print(f"Exported clean Sentis model: {output_path}")