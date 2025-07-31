// Fill out your copyright notice in the Description page of Project Settings.


#include "AppManager.h"
#include "Kismet/KismetSystemLibrary.h"

double AAppManager::lastActionTime = 0;
double AAppManager::currentFPS = 0;

void AAppManager::Tick( float deltaTime ){
  Super::Tick( deltaTime );
  if( AAppManager::currentFPS != 10 && AAppManager::lastActionTime + 5 < FPlatformTime::Seconds() ){
    UKismetSystemLibrary::ExecuteConsoleCommand( this, "t.maxfps 10" );
    AAppManager::currentFPS = 10;
  }
}

AAppManager::AAppManager(){
  this->PrimaryActorTick.bCanEverTick = true;
}

