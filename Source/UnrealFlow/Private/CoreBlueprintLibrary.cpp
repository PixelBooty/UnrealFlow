// Fill out your copyright notice in the Description page of Project Settings.


#include "CoreBlueprintLibrary.h"

#include "CommandRunner.h"
#include "DesktopPlatformModule.h"
#include "ThemeDB.h"
#include "Tween/TweenManager.h"

UThemeDB* UCoreBlueprintLibrary::ThemeDB(){
  return UThemeDB::Get();
}

bool UCoreBlueprintLibrary::IsEditorMode( UObject *worldContextContainer ){
  return worldContextContainer->GetWorld()->IsPlayInEditor() || worldContextContainer->GetWorld()->IsEditorWorld();
}

FString UCoreBlueprintLibrary::OpenFileBrowserWindow( FString title, FString fileTypes, FString cancelValue, int trimCount ){
  if( IDesktopPlatform* desktopPlatform = FDesktopPlatformModule::Get() ){
    TArray<FString> outFileNames;
    bool fileWasOpened = desktopPlatform->OpenFileDialog(
      nullptr,
      title,
      TEXT(""),
      TEXT(""),
      fileTypes,
      EFileDialogFlags::None,
      outFileNames
    );
    if( fileWasOpened ){
      FString currentDirectory = FPlatformProcess::GetCurrentWorkingDirectory();
      FString relativePath = FString::Join( outFileNames, TEXT("") );
      FString absolutePath = FPaths::ConvertRelativePathToFull( currentDirectory, relativePath );

      return UCoreBlueprintLibrary::TrimmedPaths( absolutePath, trimCount );
    }
  }

  return cancelValue;
}

ATweenManager * UCoreBlueprintLibrary::TweenManager(){
  return ATweenManager::Get();
}

ACommandRunner * UCoreBlueprintLibrary::CommandRunner(){
  return ACommandRunner::Get();
}

TArray<FString> UCoreBlueprintLibrary::SplitAndTrimString( FString string ){
  TArray<FString> resultArray;
  
  if( string.IsEmpty() ){
    return resultArray;
  }
  
  string.ParseIntoArray( resultArray, TEXT(","), true );
  for( FString& resultString : resultArray ){
    resultString = resultString.TrimStartAndEnd();
  }
    
  return resultArray;
}

FString UCoreBlueprintLibrary::TrimmedPaths( FString fullPath, int trimCount ){
  TArray<FString> resultArray;
  
  if( fullPath.IsEmpty() ){
    return "";
  }
  
  fullPath.ParseIntoArray( resultArray, TEXT("/"), true );

  for( int i = 0; i < trimCount; i++ ){
    resultArray.Pop();
  }

  return FString::Join( resultArray, TEXT("/") );
}

void UCoreBlueprintLibrary::SetThemeDB( UThemeDB *themeDB ){
  UThemeDB::SetInstance( themeDB );
}
