// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CoreApp.h"
#include "ThemeDB.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "CoreBlueprintLibrary.generated.h"

/**
 * 
 */
UCLASS()
class UNREALFLOW_API UCoreBlueprintLibrary : public UBlueprintFunctionLibrary{
  GENERATED_BODY()

public:

  UFUNCTION(BlueprintPure, meta = (Keywords = "Singlitons"))
  static UThemeDB* ThemeDB();

  UFUNCTION(BlueprintPure, meta=(WorldContext="worldContextContainer"))
  static bool IsEditorMode( UObject* worldContextContainer );

  UFUNCTION(BlueprintCallable)
  static FString OpenFileBrowserWindow( FString title, FString fileTypes, FString cancelValue, int trimCount );

  UFUNCTION(BlueprintCallable)
  static FString OpenFolderBrowserWindow( FString title, FString cancelValue );

  UFUNCTION(BlueprintPure)
  static ATweenManager* TweenManager();

  UFUNCTION(BlueprintPure)
  static ACommandRunner* CommandRunner();
  
  UFUNCTION(BlueprintPure)
  static TArray<FString> SplitAndTrimString( FString string );

  UFUNCTION(BlueprintPure)
  static FString TrimmedPaths( FString fullPath, int trimCount );

  UFUNCTION(BlueprintCallable)
  static void SetThemeDB( UThemeDB* themeDB );

  UFUNCTION(BlueprintCallable, meta=(WorldContext="worldContext"))
  static void ActivateAction( UObject* worldContext );
};
