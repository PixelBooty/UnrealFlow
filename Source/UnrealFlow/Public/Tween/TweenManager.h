// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CoreApp.h"
#include "GameFramework/Actor.h"
#include "TweenManager.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam( FOnTweenUpdate, float, currentValue );
DECLARE_DYNAMIC_DELEGATE( FOnTweenFinished );

USTRUCT()
struct FTweener{
  GENERATED_BODY()
  
  FOnTweenUpdate onUpdate;
  FOnTweenFinished onFinished;
  float currentTime = 0;
  float totalTime;
  EEase easeType;
};

UCLASS()
class UNREALFLOW_API ATweenManager : public AActor{
  GENERATED_BODY()

public:
  // Sets default values for this actor's properties
  ATweenManager();

protected:
  // Called when the game starts or when spawned
  virtual void BeginPlay() override;

  virtual void Destroyed() override;

public:
  // Called every frame
  virtual void Tick( float DeltaTime ) override;

  static ATweenManager* Get();

  UFUNCTION(BlueprintCallable)
  void StartTween( float time, EEase easeType, FOnTweenUpdate onUpdate, FOnTweenFinished onFinished );

private:
  static ATweenManager* _instance;

  TArray<FTweener> _tweeners;
};
