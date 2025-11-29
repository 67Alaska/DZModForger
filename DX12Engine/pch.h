#ifndef PCH_H
#define PCH_H

#include "framework.h"
#include <windows.h>
#include <wrl.h>
#include <wrl/client.h>
#include <wrl/implements.h>

#define D3D12_IGNORE_SDK_LAYERS

#include <d3d12.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <DirectXMath.h>
#include <d3dx12.h>

#if defined(_DEBUG)
#include <dxgidebug.h>
#endif

#endif //PCH_H