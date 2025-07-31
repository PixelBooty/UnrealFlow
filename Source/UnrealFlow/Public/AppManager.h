// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Info.h"
#include "AppManager.generated.h"

/**
 * 
 */
UCLASS()
class UNREALFLOW_API AAppManager : public AInfo{

	GENERATED_BODY()
	
protected:

	virtual void Tick( float deltaTime ) override;

public:

	AAppManager();

	static double lastActionTime;
	static double currentFPS;

private:


	
};
