// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CoreApp.h"
#include "Engine/DataAsset.h"
#include "Project.generated.h"


UCLASS(BlueprintType)
class UNREALFLOW_API UProject : public UObject{
  GENERATED_BODY()

public:
  
  UProject();

  UPROPERTY(EditAnywhere, BlueprintReadWrite)
  FString displayName;

  UPROPERTY(EditAnywhere, BlueprintReadWrite)
  FString syncName;

  UPROPERTY(EditAnywhere, BlueprintReadWrite)
  int versionsToKeep;

  UPROPERTY(EditAnywhere, BlueprintReadWrite)
  FString projectPath;

  UPROPERTY(EditAnywhere, BlueprintReadWrite)
  TArray<FString> syncPaths;
  
};
