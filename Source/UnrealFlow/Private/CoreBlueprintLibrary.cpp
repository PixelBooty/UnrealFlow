// Fill out your copyright notice in the Description page of Project Settings.


#include "CoreBlueprintLibrary.h"
#include "AppManager.h"
#include "CommandRunner.h"
#include "DesktopPlatformModule.h"
#include "ThemeDB.h"
#include "Tween/TweenManager.h"
#include "Kismet/KismetSystemLibrary.h"

#if PLATFORM_WINDOWS
#include "Windows/AllowWindowsPlatformTypes.h"
#include <windows.h>
#include <commdlg.h>
#include <shlobj.h>
#include "Windows/HideWindowsPlatformTypes.h"
#elif PLATFORM_MAC
#include <Cocoa/Cocoa.h>
#endif

UThemeDB* UCoreBlueprintLibrary::ThemeDB(){
  return UThemeDB::Get();
}

bool UCoreBlueprintLibrary::IsEditorMode( UObject *worldContextContainer ){
  return worldContextContainer->GetWorld()->IsPlayInEditor() || worldContextContainer->GetWorld()->IsEditorWorld();
}

FString UCoreBlueprintLibrary::OpenFileBrowserWindow( FString title, FString fileTypes, FString cancelValue, int trimCount ){
  FString result = cancelValue;

#if PLATFORM_WINDOWS
  OPENFILENAMEW ofn;
  WCHAR szFile[MAX_PATH] = { 0 };

  ZeroMemory(&ofn, sizeof(ofn));
  ofn.lStructSize = sizeof(ofn);
  ofn.hwndOwner = nullptr;
  ofn.lpstrFile = szFile;
  ofn.nMaxFile = sizeof( szFile ) / sizeof(WCHAR);

  FString FilterStr = fileTypes;
  FilterStr.ReplaceInline(TEXT("|"), TEXT("\0"), ESearchCase::IgnoreCase);
  FilterStr += TEXT("\0");

  ofn.lpstrFilter = *FilterStr;
  ofn.nFilterIndex = 1;
  ofn.lpstrTitle = *title;
  ofn.lpstrInitialDir = *FPlatformProcess::GetCurrentWorkingDirectory();
  ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR;

  if( GetOpenFileNameW( &ofn ) ){
    result = FString( szFile );
    result = FPaths::ConvertRelativePathToFull( result );
    result = TrimmedPaths( result, trimCount );
  }

#elif PLATFORM_MAC
  @autoreleasepool{
    NSOpenPanel* openPanel = [NSOpenPanel openPanel];
    [openPanel setTitle:[NSString stringWithFString:title]];
    [openPanel setCanChooseFiles:YES];
    [openPanel setCanChooseDirectories:NO];
    [openPanel setAllowsMultipleSelection:NO];

    // Parse file types (format: "Description|*.ext1;*.ext2")
    TArray<FString> typePairs;
    fileTypes.ParseIntoArray(typePairs, TEXT("|"));

    if( typePairs.Num() >= 2 ){
      NSMutableArray* allowedTypes = [NSMutableArray array];
      TArray<FString> extensions;
      typePairs[1].ParseIntoArray( extensions, TEXT(";") );

      for( const FString& extension : extensions ){
        FString cleanExtension = extension;
        cleanExtension.ReplaceInline(TEXT("*."), TEXT(""));
        [allowedTypes addObject:[NSString stringWithFString:cleanExtension]];
      }

      [openPanel setAllowedFileTypes:allowedTypes];
    }

    if( [openPanel runModal] == NSModalResponseOK ){
      NSURL* url = [[openPanel URLs] objectAtIndex:0];
      result = FString([url path]);
      result = TrimmedPaths( result, trimCount );
    }
  }

#elif PLATFORM_LINUX

  FString scriptPath = FPaths::Combine( FPaths::ProjectDir(), TEXT("Content/Sync/unreal_file_dialog.sh") );

  if( FPaths::FileExists( scriptPath ) ){
    // Build command arguments
    FString args = FString::Printf( TEXT("--mode file --title \"%s\""), *title );

    // Add file type filters
    TArray<FString> typePairs;
    fileTypes.ParseIntoArray( typePairs, TEXT("|") );
    if( typePairs.Num() >= 2 ){
      args += FString::Printf( TEXT(" --filter \"%s\" --extensions \"%s\""), *typePairs[0], *typePairs[1] );
    }

    // Execute the script
    FString stdOut;
    int32 returnCode;
    FPlatformProcess::ExecProcess(*scriptPath, *args, &returnCode, &stdOut, nullptr);

    if( returnCode == 0 && !stdOut.IsEmpty() ){
      result = stdOut.TrimStartAndEnd();
      result = TrimmedPaths( result, trimCount );
    }
  }
  else{
    UE_LOG(LogTemp, Error, TEXT("Linux file dialog script not found at: %s"), *scriptPath );
  }

#endif

  return result;
}

FString UCoreBlueprintLibrary::OpenFolderBrowserWindow( FString title, FString cancelValue ){
  FString result = cancelValue;

#if PLATFORM_WINDOWS
  BROWSEINFOW bi = { 0 };
  bi.lpszTitle = *title;
  bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;

  LPITEMIDLIST pidl = SHBrowseForFolderW(&bi);
  if (pidl != nullptr)
  {
    WCHAR path[MAX_PATH];
    if (SHGetPathFromIDListW(pidl, path))
    {
      result = FString(path);
      result = FPaths::ConvertRelativePathToFull( result );
    }

    // Free memory used
    IMalloc* imalloc = nullptr;
    if (SUCCEEDED(SHGetMalloc(&imalloc)))
    {
      imalloc->Free(pidl);
      imalloc->Release();
    }
  }
#elif PLATFORM_MAC
  @autoreleasepool
  {
    NSOpenPanel* openPanel = [NSOpenPanel openPanel];
    [openPanel setTitle:[NSString stringWithFString:title]];
    [openPanel setCanChooseFiles:NO];
    [openPanel setCanChooseDirectories:YES];
    [openPanel setAllowsMultipleSelection:NO];

    if ([openPanel runModal] == NSModalResponseOK)
    {
      NSURL* url = [[openPanel URLs] objectAtIndex:0];
      result = FString([url path]);
    }
  }

#elif PLATFORM_LINUX
  FString scriptPath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Content/Sync/unreal_file_dialog.sh"));

  if( FPaths::FileExists( scriptPath ) ){
    // Build command arguments
    FString args = FString::Printf( TEXT("--mode folder --title \"%s\""), *title );

    // Execute the script
    FString stdOut;
    int32 returnCode;
    FPlatformProcess::ExecProcess( *scriptPath, *args, &returnCode, &stdOut, nullptr );

    if( returnCode == 0 && !stdOut.IsEmpty() ){
      result = stdOut.TrimStartAndEnd();
    }
  }
  else{
    UE_LOG(LogTemp, Error, TEXT("Linux file dialog script not found at: %s"), *scriptPath );
  }
#endif

  return result;
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

void UCoreBlueprintLibrary::ActivateAction( UObject* worldContext ){
  UKismetSystemLibrary::ExecuteConsoleCommand( worldContext, "t.maxfps 60" );
  AAppManager::lastActionTime = FPlatformTime::Seconds();
  AAppManager::currentFPS = 60;
}
