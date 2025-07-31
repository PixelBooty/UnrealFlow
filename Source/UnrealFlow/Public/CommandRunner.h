// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreApp.h"
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"

#if PLATFORM_WINDOWS
#include <windows.h>
#else // macOS and Linux (POSIX)
#include <unistd.h>
#include <sys/wait.h>
#include <fcntl.h>
#endif

#include "CommandRunner.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam( FOnCommandOutput, const FString&, output );
DECLARE_DYNAMIC_DELEGATE( FOnCommandCompleted );

UENUM( BlueprintType )
enum class ESystemType : uint8{
  Windows,
  Linux,
  MacOS
};

UCLASS()
class UNREALFLOW_API ACommandRunner : public AActor{
  GENERATED_BODY()

protected:
  
  virtual void BeginPlay() override;

  virtual void Destroyed() override;
  
public:
  
  ACommandRunner();

  static ACommandRunner* Get();
  
  virtual void Tick( float deltaTime ) override;

  UFUNCTION(BlueprintCallable)
  void ExecuteCommand( FString command, FOnCommandOutput outputEvent, FOnCommandCompleted completed );

  UFUNCTION(BlueprintCallable)
  void ExecuteCommandInWindow( FString command );

  UPROPERTY(BlueprintReadOnly)
  ESystemType systemType;

  bool IsRunning();
  bool Start( FString command );
  void Stop();

  UFUNCTION(BlueprintCallable)
  void OpenFile( FString program, FString filePath );

private:

  static ACommandRunner* _instance;

  FOnCommandOutput _onCommandOutput;
  FOnCommandCompleted _onCommandCompleted;
  bool wasRunning;

  void _ReadOutput();

  

#if PLATFORM_WINDOWS
  HANDLE _readPipe = nullptr;
  HANDLE _writePipe = nullptr;
  PROCESS_INFORMATION _processInfo = { 0 };
#else
  FILE* _pipe;
#endif
  
};
