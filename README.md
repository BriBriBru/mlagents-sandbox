# ML-Agents Sandbox

Fun and personal projects to build and reinforce ML-Agents and Unity 3D skills.

## Prerequisites

* [Python 3.10.0](https://www.python.org/downloads/release/python-3100/)
* [Unity 2022.3.23f1](https://download.unity3d.com/download_unity/dbb3f7c5b5c6/Windows64EditorInstaller/UnitySetup64-2022.3.23f1.exe)

## Setup ML-Agents

1) Install the "ml-agents" package in the Unity editor
2) Open the terminal at the root of the project
3) Check that the Python version is 3.10.0 (then type "exit()" to exit Python)
   ```shell
   py | python 
   ```
4) Create a Python virtual environment
   ```shell
   py -m venv [environment name]
   ```
5) Activate the virtual environment
   ```shell
   .\[environment name]\Scripts\activate
   ```
6) Check and upgrade pip (package installer)
   ```shell
   py -m pip install --upgrade pip
   ```
7) Install ML-Agents
   ```shell
   pip install mlagents
   ```
8) Install the Torch machine learning library
   ```shell
   pip install torch torchvision torchaudio
   ```
9) Verify that everything is working correctly
   ```shell
   mlagents-learn --help
   ```
If it doesn't work, good luck!
Fix all errors until the command in step 9) doesn't display any error

Useful commands in case of errors:
* Protobuf
  ```shell
  pip install protobuf==3.20.3
  ```
* Packaging
  ```shell
  pip install packaging
  ```
* Numpy
  ```shell
  pip install numpy
  ```

## Run Training

* Start training
  ```shell
  mlagents-learn --run-id [run name]
  ```
* Resume training
  ```shell
  mlagents-learn --run-id [run name to resume] --resume
  ```
* In case of "Module onnx is not installed!" error:
  ```shell
  pip install onnx
  ```

