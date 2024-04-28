# ML-Agents Sandbox

Fun and personal projects to build and reinforce ML-Agents and Unity 3D skills.

## Prerequisites

* [Python 3.10.0](https://www.python.org/downloads/release/python-3100/)
* [Unity 2022.3.23f1](https://download.unity3d.com/download_unity/dbb3f7c5b5c6/Windows64EditorInstaller/UnitySetup64-2022.3.23f1.exe)

## Setup ML-Agents

OS used : Windows 11

**1) Install the "ml-agents" package in the Unity editor**

**2) Open the terminal at the root of the project**

**3) Check that the Python version is 3.10.0 (then type "exit()" to exit Python)**
```shell
py | python 
```

**4) Create a Python virtual environment**
```shell
py -m venv [environment name]
```

**5) Activate the virtual environment**
```shell
.\[environment name]\Scripts\activate
```
If your system doesn't allow script execution, run the following command in PowerShell as administrator
```shell
Set-ExecutionPolicy RemoteSigned
```

**6) Check and upgrade pip (package installer)**
```shell
py -m pip install --upgrade pip
```

**7) Install Packaging module**
```shell
pip install packaging
```

**8) Install Numpy 1.21.2**
```shell
pip install numpy==1.21.2
```
You might get this error : "error: Microsoft Visual C++ 14.0 is required.",
I saw that this could be fixed by installing the software [Visual Studio](https://visualstudio.microsoft.com/fr/vs/) with the module "Desktop development with C++"

**9) Install ML-Agents**
```shell
pip install mlagents
```

**10) Install the Torch machine learning library**
```shell
pip install torch torchvision torchaudio
```

**11) Change Protobuf version to 3.20.3**
```shell
pip install protobuf==3.20.3
```

**12) Install ONNX packages**
```shell
pip install onnx
```

**13) Verify that everything is working correctly**
```shell
mlagents-learn --help
```

If it doesn't work, good luck!
Fix all errors until the command in step 9) doesn't display any error

## Run Training

**Start training**
```shell
mlagents-learn --run-id [run name]
```
**Resume training**
```shell
mlagents-learn --run-id [run name to resume] --resume
```

