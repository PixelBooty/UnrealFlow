// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CoreApp.h"
#include "Engine/DataAsset.h"
#include "ThemeDB.generated.h"

UENUM(BlueprintType)
enum class EThemeType : uint8{
  Light,
  Dark
};

UENUM(BlueprintType)
enum class EThemeColor : uint8{
  Primary,
  Secondary,
  Success,
  Warning,
  Error,
  Info,
  Surface,
  Font,
  Link
};

UENUM(BlueprintType)
enum class EThemeShades : uint8{
  A0,
  A10,
  A20,
  A30,
  A40,
  A50
};

USTRUCT(BlueprintType)
struct FThemeColor{
  GENERATED_BODY()

  UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
  FLinearColor color;

  UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
  EThemeShades fontShade;
  
};

USTRUCT(BlueprintType)
struct FColorMap{
  GENERATED_BODY()

  UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
  TMap<EThemeShades, FThemeColor> shades;
  
};

USTRUCT(BlueprintType)
struct FColorStyle{
  GENERATED_BODY()

  UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
  EThemeColor color;

  UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
  EThemeShades shade;
  
};

/**
 * 
 */
UCLASS(BlueprintType)
class UNREALFLOW_API UThemeDB : public UDataAsset{
  GENERATED_BODY()

protected:

public:

  UThemeDB();

  static void SetInstance( UThemeDB* themeDB );
  
  static UThemeDB* Get();

  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  TMap<EThemeColor, FColorMap> lightTheme;

  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  TMap<EThemeColor, FColorMap> darkTheme;

  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  FColorMap lightSurface;
  
  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  FColorMap darkSurface;

  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  TMap<EThemeShades, FLinearColor> font;

  UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
  EThemeType theme;

  UFUNCTION(BlueprintPure)
  const TMap<EThemeColor, FColorMap>& ColorMap() const;
  
  UFUNCTION(BlueprintPure)
  const FColorMap& Surface() const;

  UFUNCTION(BlueprintPure)
  FThemeColor ColorFromStyle( const FColorStyle& style ) const;
  
private:

  

  static UThemeDB* _instance;
};
