// (C) 2024 Pixel Booty LLC. All rights reserved. Unauthorized use or distribution prohibited.

#pragma once

#include "CoreApp.h"
#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "Easings.generated.h"

//https://easings.net/#:~:text=Easing%20Functions%20Cheat%20Sheet.%20Easing%20functions%20specify%20the%20rate%20of

UENUM(BlueprintType)
enum class EEase : uint8{
  Linear,
  Custom,
  EaseInQuad,
  EaseOutQuad,
  EaseInOutQuad,
  EaseInCubic,
  EaseOutCubic,
  EaseInOutCubic,
  EaseInQuart,
  EaseOutQuart,
  EaseInOutQuart,
  EaseInQuint,
  EaseOutQuint,
  EaseInOutQuint,
  EaseInSine,
  EaseOutSine,
  EaseInOutSine,
  EaseInExpo,
  EaseOutExpo,
  EaseInOutExpo,
  EaseInCirc,
  EaseOutCirc,
  EaseInOutCirc,
  Spring,
  EaseInBounce,
  EaseOutBounce,
  EaseInOutBounce,
  EaseInBack,
  EaseOutBack,
  EaseInOutBack,
  EaseInElastic,
  EaseOutElastic,
  EaseInOutElastic
};

UCLASS()
class UNREALFLOW_API UEasings : public UBlueprintFunctionLibrary{
	GENERATED_BODY()

public:
  
  static constexpr double Pi = 3.1415926535897932384626433832795;

  static double EaseValue( double value, EEase easeMethod );

	UFUNCTION(BlueprintCallable, Category="Easings")
	static double Linear( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double Spring( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInQuad( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutQuad( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutQuad( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInCubic( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutCubic( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutCubic( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInQuart( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutQuart( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutQuart( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInQuint( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutQuint( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutQuint( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInSine( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutSine( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutSine( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInExpo( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutExpo( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutExpo( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInCirc( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutCirc( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutCirc( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInBounce( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutBounce( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutBounce( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInBack( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutBack( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutBack( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInElastic( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseOutElastic( double start, double end, double value );

  UFUNCTION(BlueprintCallable, Category="Easings")
  static double EaseInOutElastic( double start, double end, double value );
  
};