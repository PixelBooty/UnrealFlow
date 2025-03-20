// (C) 2024 Pixel Booty LLC. All rights reserved. Unauthorized use or distribution prohibited.


#include "Tween/Easings.h"

double UEasings::EaseValue( double value, EEase easeMethod ){
  switch( easeMethod ){
  case EEase::Linear:
    return UEasings::Linear( 0, 1, value );
  case EEase::EaseInQuad:
    return UEasings::EaseInQuad( 0, 1, value );
  case EEase::EaseOutQuad:
    return UEasings::EaseOutQuad( 0, 1, value );
  case EEase::EaseInOutQuad:
    return UEasings::EaseInOutQuad( 0, 1, value );
  case EEase::EaseInCubic:
    return UEasings::EaseInCubic( 0, 1, value );
  case EEase::EaseOutCubic:
    return UEasings::EaseOutCubic( 0, 1, value );
  case EEase::EaseInOutCubic:
    return UEasings::EaseInOutCubic( 0, 1, value );
  case EEase::EaseInQuart:
    return UEasings::EaseInQuart( 0, 1, value );
  case EEase::EaseOutQuart:
    return UEasings::EaseOutQuart( 0, 1, value );
  case EEase::EaseInOutQuart:
    return UEasings::EaseInOutQuart( 0, 1, value );
  case EEase::EaseInQuint:
    return UEasings::EaseInQuint( 0, 1, value );
  case EEase::EaseOutQuint:
    return UEasings::EaseOutQuint( 0, 1, value );
  case EEase::EaseInOutQuint:
    return UEasings::EaseInOutQuint( 0, 1, value );
  case EEase::EaseInSine:
    return UEasings::EaseInSine( 0, 1, value );
  case EEase::EaseOutSine:
    return UEasings::EaseOutSine( 0, 1, value );
  case EEase::EaseInOutSine:
    return UEasings::EaseInOutSine( 0, 1, value );
  case EEase::EaseInExpo:
    return UEasings::EaseInExpo( 0, 1, value );
  case EEase::EaseOutExpo:
    return UEasings::EaseOutExpo( 0, 1, value );
  case EEase::EaseInOutExpo:
    return UEasings::EaseInOutExpo( 0, 1, value );
  case EEase::EaseInCirc:
    return UEasings::EaseInCirc( 0, 1, value );
  case EEase::EaseOutCirc:
    return UEasings::EaseOutCirc( 0, 1, value );
  case EEase::EaseInOutCirc:
    return UEasings::EaseInOutCirc( 0, 1, value );
  case EEase::Spring:
    return UEasings::Spring( 0, 1, value );
  case EEase::EaseInBounce:
    return UEasings::EaseInBounce( 0, 1, value );
  case EEase::EaseOutBounce:
    return UEasings::EaseOutBounce( 0, 1, value );
  case EEase::EaseInOutBounce:
    return UEasings::EaseInOutBounce( 0, 1, value );
  case EEase::EaseInBack:
    return UEasings::EaseInBack( 0, 1, value );
  case EEase::EaseOutBack:
    return UEasings::EaseOutBack( 0, 1, value );
  case EEase::EaseInOutBack:
    return UEasings::EaseInOutBack( 0, 1, value );
  case EEase::EaseInElastic:
    return UEasings::EaseInElastic( 0, 1, value );
  case EEase::EaseOutElastic:
    return UEasings::EaseOutElastic( 0, 1, value );
  case EEase::EaseInOutElastic:
    return UEasings::EaseInOutElastic( 0, 1, value );
  default:
    return UEasings::Linear( 0, 1, value );
  }
}

double UEasings::Linear( double start, double end, double value ){
  return start + value * ( end - start );
}

double UEasings::Spring( double start, double end, double value ){
  value = FMath::Clamp( value, 0, 1 );
  value = (
    ( FMath::Sin( value * UEasings::Pi * ( 0.2 + ( 2.5 * value * value * value ) ) ) * FMath::Pow( 1 - value, 2.2 ) )
    + value
  ) * ( 1 + ( 1.2 * ( 1 - value ) ) );
  return start + ( ( end - start ) * value );
}

double UEasings::EaseInQuad( double start, double end, double value ){
  end -= start;
  return ( end * value * value ) + start;
}

double UEasings::EaseOutQuad( double start, double end, double value ){
  end -= start;
  return ( -end * value * ( value - 2 ) ) + start;
}

double UEasings::EaseInOutQuad( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return ( end * 0.5f * value * value ) + start;
  }

  value--;
  return ( -end * 0.5f * ( ( value * ( value - 2 ) ) - 1 ) ) + start;
}

double UEasings::EaseInCubic( double start, double end, double value ){
  end -= start;
  return ( end * value * value * value ) + start;
}

double UEasings::EaseOutCubic( double start, double end, double value ){
  value--;
  end -= start;
  return ( end * ( ( value * value * value ) + 1 ) ) + start;
}

double UEasings::EaseInOutCubic( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return ( end * 0.5f * value * value * value ) + start;
  }

  value -= 2;
  return ( end * 0.5f * ( ( value * value * value ) + 2 ) ) + start;
}

double UEasings::EaseInQuart( double start, double end, double value ){
  end -= start;
  return ( end * value * value * value * value ) + start;
}

double UEasings::EaseOutQuart( double start, double end, double value ){
  value--;
  end -= start;
  return ( -end * ( (value * value * value * value ) - 1 ) ) + start;
}

double UEasings::EaseInOutQuart( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return (end * 0.5f * value * value * value * value) + start;
  }

  value -= 2;
  return (-end * 0.5f * ((value * value * value * value) - 2)) + start;
}

double UEasings::EaseInQuint( double start, double end, double value ){
  end -= start;
  return ( end * value * value * value * value * value ) + start;
}

double UEasings::EaseOutQuint( double start, double end, double value ){
  value--;
  end -= start;
  return ( end * ( ( value * value * value * value * value ) + 1 ) ) + start;
}

double UEasings::EaseInOutQuint( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return ( end * 0.5f * value * value * value * value * value ) + start;
  }

  value -= 2;
  return ( end * 0.5f * ((value * value * value * value * value ) + 2 ) ) + start;
}

double UEasings::EaseInSine( double start, double end, double value ){
  end -= start;
  return ( -end * FMath::Cos( value * ( UEasings::Pi * 0.5 ) ) ) + end + start;
}

double UEasings::EaseOutSine( double start, double end, double value ){
  end -= start;
  return ( end * FMath::Sin( value * ( UEasings::Pi * 0.5 ) ) ) + start;
}

double UEasings::EaseInOutSine( double start, double end, double value ){
  end -= start;
  return ( -end * 0.5f * ( FMath::Cos( UEasings::Pi * value ) - 1 ) ) + start;
}

double UEasings::EaseInExpo( double start, double end, double value ){
  end -= start;
  return ( end * FMath::Pow( 2, 10 * ( value - 1 ) ) ) + start;
}

double UEasings::EaseOutExpo( double start, double end, double value ){
  end -= start;
  return ( end * ( -FMath::Pow( 2, -10 * value ) + 1 ) ) + start;
}

double UEasings::EaseInOutExpo( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return ( end * 0.5 * FMath::Pow( 2, 10 * ( value - 1 ) ) ) + start;
  }

  value--;
  return ( end * 0.5 * ( -FMath::Pow( 2, -10 * value ) + 2 ) ) + start;
}

double UEasings::EaseInCirc( double start, double end, double value ){
  end -= start;
  return ( -end * ( FMath::Sqrt( 1 - ( value * value ) ) - 1 ) ) + start;
}

double UEasings::EaseOutCirc( double start, double end, double value ){
  value--;
  end -= start;
  return ( end * FMath::Sqrt( 1 - ( value * value ) ) ) + start;
}

double UEasings::EaseInOutCirc( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < 1 ) {
    return ( -end * 0.5 * ( FMath::Sqrt( 1 - ( value * value ) ) - 1 ) ) + start;
  }

  value -= 2;
  return ( end * 0.5 * ( FMath::Sqrt( 1 - ( value * value ) ) + 1 ) ) + start;
}

double UEasings::EaseInBounce( double start, double end, double value ){
  end -= start;
  return end - UEasings::EaseOutBounce( 0, end, 1 - value ) + start;
}

double UEasings::EaseOutBounce( double start, double end, double value ){
  value *= 2;
  end -= start;
  if( value < ( 1 / 2.75 ) ) {
    return ( end * ( 7.5625 * value * value ) ) + start;
  }

  if( value < (2 / 2.75) ) {
    value -= 1.5 / 2.75;
    return ( end * ( ( 7.5625 * value * value ) + .75 ) ) + start;
  }
  if( value < (2.5 / 2.75) ) {
    value -= 2.25 / 2.75;
    return ( end * ( ( 7.5625 * value * value ) + .9375 ) ) + start;
  }
  value -= 2.625 / 2.75;
  return ( end * ( ( 7.5625 * value * value ) + .984375 ) ) + start;
}

double UEasings::EaseInOutBounce( double start, double end, double value ){
  end -= start;
  return value < 0.5
    ? ( UEasings::EaseInBounce( 0, end, value * 2 ) * 0.5 ) + start
    : ( UEasings::EaseOutBounce( 0, end, ( value * 2 ) - 1 ) * 0.5) + ( end * 0.5 ) + start;
}

double UEasings::EaseInBack( double start, double end, double value ){
  end -= start;
  value *= 2;
  return ( end * value * value * ( ( ( 1.70158 + 1 ) * value ) - 1.70158 ) ) + start;
}

double UEasings::EaseOutBack( double start, double end, double value ){
  end -= start;
  value -= 1;
  return ( end * ( ( value * value * ( ( ( 1.70158 + 1 ) * value ) + 1.70158 ) ) + 1 ) ) + start;
}

double UEasings::EaseInOutBack( double start, double end, double value ){
  double s = 1.70158;
  end -= start;
  value *= 2;
  if( value < 1 ) {
    s *= 1.525;
    return ( end * 0.5 * ( value * value * ( ( ( s + 1 ) * value ) - s ) ) ) + start;
  }

  value -= 2;
  s *= 1.525;
  return ( end * 0.5 * ( ( value * value * ( ( ( s + 1 ) * value ) + s ) ) + 2 ) ) + start;
}

double UEasings::EaseInElastic( double start, double end, double value ){
  end -= start;

  if( FMath::IsNearlyZero( value ) ) {
    return start;
  }

  if( FMath::IsNearlyZero( value - 1 ) ) {
    return start + end;
  }

  return -(
    end * FMath::Pow( 2, 10 * ( value - 1 ) ) * FMath::Sin( ( ( value * 1 ) - .075 ) * ( 2 * UEasings::Pi ) / .3 )
  ) + start;
}

double UEasings::EaseOutElastic( double start, double end, double value ){
  end -= start;

  if( FMath::IsNearlyZero( value ) ) {
    return start;
  }

  if( FMath::IsNearlyZero( value - 1 ) ) {
    return start + end;
  }

  return (
    end * FMath::Pow( 2, -10 * value ) * FMath::Sin( ( ( value * 1 ) - .075 ) * ( 2 * UEasings::Pi ) / .3 )
  ) + end + start;
}

double UEasings::EaseInOutElastic( double start, double end, double value ){
  end -= start;

  if( FMath::IsNearlyZero( value ) ) {
    return start;
  }

  if( FMath::IsNearlyZero( (value *= 2 ) - 2 ) ) {
    return start + end;
  }

  return value < 1
    ? (
      -.5 * (
        end * FMath::Pow( 2, 10 * ( value - 1 ) ) * FMath::Sin( ( ( value * 1 ) - .075 ) * ( 2 * UEasings::Pi ) / .3 )
      )
    ) + start
    : (
      end * FMath::Pow( 2, -10 * ( value - 1 ) ) * FMath::Sin( ( ( value * 1 ) - .075 ) * ( 2 * UEasings::Pi ) / .3 ) * .5
    ) + end + start;
}