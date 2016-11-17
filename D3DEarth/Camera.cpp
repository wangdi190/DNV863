#include "StdAfx.h"
#include "Camera.h"
#include "float.h"


CCamera::CCamera(void)
{
	isAni=false;
}


CCamera::~CCamera(void)
{
}

HRESULT 
CCamera::Create(D3DXVECTOR3 position,D3DXVECTOR3 lookat, D3DXVECTOR3 up, float fieldofview, float dnear, float dfar,float aspect, CCamera **ppCamera)
{
    HRESULT hr = S_OK;

    CCamera *pCamera = new CCamera();
    IFCOOM(pCamera);

	pCamera->curpara.cameraPosition= position;
	pCamera->curpara.cameraLookat=lookat;
	pCamera->curpara.cameraDirection= lookat-position;
	pCamera->curpara.cameraUp=up;
	pCamera->curpara.FieldOfView=fieldofview;
	pCamera->curpara.Near=dnear;
	pCamera->curpara.Far=dfar;
	pCamera->Aspect=aspect;

    IFC(pCamera->Update());

    *ppCamera = pCamera;
    pCamera = NULL;

Cleanup:
    delete pCamera;

    return hr;
}

HRESULT 
CCamera::Update()
{
    HRESULT hr = S_OK;

	D3DXMatrixPerspectiveFovRH(&projection, curpara.FieldOfView, Aspect, curpara.Near, curpara.Far);



	//D3DXVec3Normalize(&cameraDirection,&cameraDirection);

	//D3DXVECTOR3 lookat=cameraPosition+cameraDirection;

	D3DXMatrixLookAtRH(&view, &curpara.cameraPosition, &curpara.cameraLookat, &curpara.cameraUp);


	//D3DXMATRIX scale;
	//D3DXMatrixScaling(&scale,1,1,-1);
	//D3DXMatrixMultiply(&view,&view,&scale);



Cleanup:

    return hr;
}

//-------------------------------------------------------------------------
// 计算动画中的参数
//-------------------------------------------------------------------------
void
CCamera::aniCamera(float progress)
{
		D3DXVec3Lerp(&curpara.cameraPosition,&oldpara.cameraPosition,&newpara.cameraPosition,progress);
		D3DXVec3Lerp(&curpara.cameraLookat,&oldpara.cameraLookat,&newpara.cameraLookat,progress);
		D3DXVec3Lerp(&curpara.cameraUp,&oldpara.cameraUp,&newpara.cameraUp,progress);
		curpara.Near=oldpara.Near+(newpara.Near-oldpara.Near)*progress;
		curpara.Far=oldpara.Far+(newpara.Far-oldpara.Far)*progress;
	
	if (_isnan(curpara.cameraPosition.x))
	{
		curpara.cameraPosition=newpara.cameraPosition;
		curpara.cameraLookat=newpara.cameraLookat;
		curpara.cameraUp=newpara.cameraUp;
		curpara.Near=newpara.Near;
		curpara.Far=newpara.Far;
	}


	Update();
}