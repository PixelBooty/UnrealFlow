// Fill out your copyright notice in the Description page of Project Settings.


#include "UnrealFlowInstance.h"
#include "GameFramework/GameUserSettings.h"

void UUnrealFlowInstance::Init(){

  Super::Init();

  // Get the Game User Settings
  UGameUserSettings* gameUserSettings = GEngine->GetGameUserSettings();
  if( gameUserSettings ){
    gameUserSettings->SetScreenResolution( FIntPoint( 800, 600 ) );
    gameUserSettings->SetFullscreenMode( EWindowMode::Windowed );
    gameUserSettings->SetWindowPosition( 200, 100 );
    gameUserSettings->ApplySettings( true );
  }

}
