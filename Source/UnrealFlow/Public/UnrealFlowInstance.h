// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "UnrealFlowInstance.generated.h"

/**
 * 
 */
UCLASS()
class UNREALFLOW_API UUnrealFlowInstance : public UGameInstance{

	GENERATED_BODY()

protected:

	virtual void Init() override;

public:

private:
	
};
