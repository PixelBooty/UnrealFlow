// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CoreApp.h"
#include "ThemeDB.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "CoreBlueprintLibrary.generated.h"

USTRUCT(BlueprintType)
struct FFileInfo
{
  GENERATED_BODY()

  UPROPERTY(BlueprintReadWrite)
  FString FileName;

  UPROPERTY(BlueprintReadWrite)
  FString FullPath;

  UPROPERTY(BlueprintReadWrite)
  bool bIsDirectory;

  UPROPERTY(BlueprintReadWrite)
  int64 FileSize;

  UPROPERTY(BlueprintReadWrite)
  FDateTime DateModified;

  UPROPERTY(BlueprintReadWrite)
  UTexture2D* Icon;
};

USTRUCT(BlueprintType)
struct FLockRow{
  GENERATED_BODY()
  
  FLockRow(){}
  
  FLockRow( const FString& fromString ){
    TArray<FString> parsedArray;
    fromString.ParseIntoArray( parsedArray, TEXT(","), true );
    
    if( parsedArray.Num() >= 3 ){
      user = parsedArray[0];
      FDateTime::Parse( parsedArray[1], lockTime );
      FDateTime::Parse( parsedArray[2], unlockTime );
    }
  }
  
  UPROPERTY(BlueprintReadWrite)
  FString user = TEXT("");
  
  UPROPERTY(BlueprintReadWrite)
  FDateTime lockTime = FDateTime::MinValue();
  
  UPROPERTY(BlueprintReadWrite)
  FDateTime unlockTime = FDateTime::MinValue();
  
  FString GetAsString() const{
    return FString::Printf( TEXT("%s,%s,%s"), *user, *lockTime.ToString(), *unlockTime.ToString() );
  }
};

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

  UFUNCTION(BlueprintCallable)
  static TArray<FFileInfo> GetFilesAtPath( FString path, bool bIncludeDirectories = true, bool bRecursive = false );

  UFUNCTION(BlueprintCallable)
  static void OpenInSystemExplorer( FString path, bool bSelectFile = false );

  UFUNCTION(BlueprintCallable)
  static FString GetFileContentsAsText( FString filePath );

  UFUNCTION(BlueprintCallable)
  static bool WriteTextToFile( FString filePath, FString textContent, bool bAppend = false );
  
  UFUNCTION(BlueprintCallable)
  static void WriteSyncLock( const FString& lockFilePath, const TArray<FLockRow> &lockRows );
  
  UFUNCTION(BlueprintCallable)
  static TArray<FLockRow> ReadSyncLock( const FString& lockFilePath );

private:
  static UTexture2D* GetFileIcon( const FString& filePath, bool bIsDirectory );
};
