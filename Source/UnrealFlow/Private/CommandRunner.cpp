// Fill out your copyright notice in the Description page of Project Settings.


#include "CommandRunner.h"
#include <string>
#include <vector>
#if PLATFORM_WINDOWS
#include <shellapi.h>
#else
#include <cstdlib>
#endif

ACommandRunner* ACommandRunner::_instance = nullptr;

ACommandRunner::ACommandRunner(){
  PrimaryActorTick.bCanEverTick = true;
  this->wasRunning = false;
}

ACommandRunner* ACommandRunner::Get(){
  return ACommandRunner::_instance;
}

void ACommandRunner::BeginPlay(){
  Super::BeginPlay();

  ACommandRunner::_instance = this;
  
#if PLATFORM_WINDOWS
  this->systemType = ESystemType::Windows;
#elif PLATFORM_LINUX
  this->systemType = ESystemType::Linux;
#elif PLATFORM_MAC
  this->systemType = ESystemType::MacOS;
#endif
  
}

void ACommandRunner::Destroyed(){
  Super::Destroyed();

  ACommandRunner::_instance = nullptr;
  this->Stop();
}

void ACommandRunner::Tick( float deltaTime ){
  Super::Tick( deltaTime );

  if( !this->IsRunning() && this->wasRunning ){
    if( this->_onCommandCompleted.IsBound() ){
      this->_onCommandCompleted.Execute();
    }
    this->wasRunning = false;
    this->_ReadOutput();
  }
  else if( this->IsRunning() ){
    this->_ReadOutput();
  }

}

void ACommandRunner::ExecuteCommand( FString command, FOnCommandOutput outputEvent, FOnCommandCompleted completed ){
  if( !this->IsRunning() ){
    this->_onCommandOutput = outputEvent;
    this->_onCommandCompleted = completed;
    this->Start( command );
    this->_ReadOutput();
  }
}

bool ACommandRunner::IsRunning(){
#if PLATFORM_WINDOWS
  if( this->_processInfo.hProcess ){
    DWORD exitCode;
    if(GetExitCodeProcess( this->_processInfo.hProcess, &exitCode ) ){
      return exitCode == STILL_ACTIVE;
    }
  }
  return false;
#else
  return false;
#endif
}

bool ACommandRunner::Start( FString command ){
#if PLATFORM_WINDOWS
  SECURITY_ATTRIBUTES securityAttributes = { sizeof( securityAttributes ), nullptr, TRUE };
  if(!CreatePipe( &this->_readPipe, &this->_writePipe, &securityAttributes, 0 ) ){
    return false;
  }

  STARTUPINFOW startupInflow = { sizeof( startupInflow ) };
  startupInflow.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW;
  startupInflow.wShowWindow = SW_HIDE;
  startupInflow.hStdOutput = this->_writePipe;
  startupInflow.hStdError = this->_writePipe;

  std::wstring cmd = std::wstring( TCHAR_TO_WCHAR(*command ) );

  UE_LOG( LogTemp, Error, TEXT("%s"), *command );

  if( !CreateProcessW( nullptr, &cmd[0], nullptr, nullptr, TRUE, CREATE_NO_WINDOW, nullptr, nullptr, &startupInflow, &this->_processInfo ) ){
    CloseHandle( this->_readPipe);
    CloseHandle( this->_writePipe );
    this->_readPipe = nullptr;
    return false;
  }

  CloseHandle( this->_writePipe ); // Close write end so we can read
  this->_writePipe = nullptr;
  this->wasRunning = true;
  return true;
#else
  return false;
#endif
}

void ACommandRunner::Stop(){
#if PLATFORM_WINDOWS
  if( this->_processInfo.hProcess ){
    TerminateProcess( this->_processInfo.hProcess, 1 );
    CloseHandle( this->_processInfo.hProcess );
    CloseHandle( this->_processInfo.hThread );
    this->_processInfo.hProcess = nullptr;
  }
  if( this->_readPipe ){
    CloseHandle( this->_readPipe );
    CloseHandle( this->_writePipe );
    this->_readPipe = nullptr;
  }
#else
  
#endif
  this->wasRunning = false;
}

void ACommandRunner::OpenFile( FString program, FString filePath ){
#if PLATFORM_WINDOWS
  ShellExecuteW(
    nullptr,
    L"open",
    *program,
    *filePath,
    nullptr,
    SW_SHOWNORMAL
  );
#elif PLATFORM_MAC
  //FString Command = FString::Printf(TEXT("open -a \"%s\" \"%s\" &"), *program, *filePath );
  //int result = system( TCHAR_TO_UTF8( *Command ) );
  //UE_LOG( LogTemp, Error, TEXT( "%i" ), result );
#elif PLATFORM_LINUX
  FString Command = FString::Printf(TEXT("%s \"%s\" &"), *program, *filePath );
  system( TCHAR_TO_UTF8( *Command ) );
#endif
}

void ACommandRunner::_ReadOutput(){
#if PLATFORM_WINDOWS
  DWORD bytesAvail = 0;
  if( PeekNamedPipe( this->_readPipe, nullptr, 0, nullptr, &bytesAvail, nullptr) && bytesAvail > 0 ){
    std::vector<char> buffer( bytesAvail + 1 );
    DWORD bytesRead;
    if( ReadFile( this->_readPipe, buffer.data(), bytesAvail, &bytesRead, nullptr ) ){
      buffer[bytesRead] = '\0';
      FString output = UTF8_TO_TCHAR( buffer.data() );
      if( this->_onCommandOutput.IsBound()){
        this->_onCommandOutput.Execute(output);
      }
    }
  }
#else
#endif
}

