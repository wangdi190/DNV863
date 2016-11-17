#pragma once
class CCamera
{
public:
	static HRESULT Create(D3DXVECTOR3 position,D3DXVECTOR3 lookat, D3DXVECTOR3 up, float fieldofview, float dnear, float dfar,float aspect, CCamera **ppCamera);
	~CCamera(void);

	D3DXMATRIXA16 view;
	D3DXMATRIXA16 projection;

	//D3DXVECTOR3 cameraPosition;
	//D3DXVECTOR3 cameraLookat;
	//D3DXVECTOR3 cameraDirection;
	//D3DXVECTOR3 cameraUp;

	//float FieldOfView;
	//float Near;
	//float Far;

	STRUCT_Camera curpara;
	STRUCT_Camera oldpara;
	STRUCT_Camera newpara;

	float Aspect;

	HRESULT Update();

	//∂Øª≠œ‡πÿ
	bool isAni;
	int duration;
	UINT startTick;
	void aniCamera(float progress);

private:
	CCamera(void);

};

