// Fill out your copyright notice in the Description page of Project Settings.


#include "Tween/TweenManager.h"
#include "Tween/Easings.h"

ATweenManager* ATweenManager::_instance = nullptr;

ATweenManager::ATweenManager(){
  this->PrimaryActorTick.bCanEverTick = true;
}

void ATweenManager::BeginPlay(){
  Super::BeginPlay();

  ATweenManager::_instance = this;
  
}

void ATweenManager::Destroyed(){
  Super::Destroyed();

  ATweenManager::_instance = nullptr;
}

void ATweenManager::Tick( float DeltaTime ){
  Super::Tick( DeltaTime );

  int tweenerToRemove = -1;
  
  for( int i = 0; i < this->_tweeners.Num(); i++ ){
    FTweener& tweener = this->_tweeners[i];
    tweener.currentTime += DeltaTime;
    float distance = FMath::Clamp( tweener.currentTime / tweener.totalTime, 0, 1 );
    if( distance < 1 ){
      float easedDistance = UEasings::EaseValue( distance, tweener.easeType );
      if( tweener.onUpdate.IsBound() ){
        tweener.onUpdate.Execute( easedDistance );
      }
    }
    else{
      if( tweener.onUpdate.IsBound() ){
        tweener.onUpdate.Execute( 1 );
      }
      if( tweener.onFinished.IsBound() ){
        tweener.onFinished.Execute();
        tweenerToRemove = i;
      }
    }
    
  }

  if( tweenerToRemove != -1 ){
    this->_tweeners.RemoveAt( tweenerToRemove );
  }
}

ATweenManager * ATweenManager::Get(){
  return ATweenManager::_instance;
}

void ATweenManager::StartTween( float time, EEase easeType, FOnTweenUpdate onUpdate, FOnTweenFinished onFinished ){

  FTweener newTweener;
  newTweener.currentTime = 0;
  newTweener.totalTime = time;
  newTweener.easeType = easeType;
  newTweener.onUpdate = onUpdate;
  newTweener.onFinished = onFinished;
  this->_tweeners.Add( newTweener );

}

