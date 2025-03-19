// Fill out your copyright notice in the Description page of Project Settings.


#include "ThemeDB.h"

UThemeDB::UThemeDB(){
  this->theme = EThemeType::Dark;
}

void UThemeDB::SetInstance( UThemeDB *themeDB ){
  UThemeDB::_instance = themeDB;
}

UThemeDB* UThemeDB::Get(){
  if( UThemeDB::_instance ){
    return UThemeDB::_instance;
  }
  else{
    UE_LOG( LogTemp, Warning, TEXT( "Generating new theme db" ) );
    FString dbLocation = TEXT("/Game/StaticAssets/DA_ThemeDB.DA_ThemeDB");
    UThemeDB::_instance = Cast<UThemeDB>( StaticLoadObject( UThemeDB::StaticClass(), nullptr, *dbLocation ) );
    return UThemeDB::_instance;
  }
}

UThemeDB* UThemeDB::_instance = nullptr;

const TMap<EThemeColor, FColorMap>& UThemeDB::ColorMap() const{
  switch( this->theme ){
  case EThemeType::Light:
    return this->lightTheme;
  case EThemeType::Dark:
    return this->darkTheme;
  default:
    return this->lightTheme;
  }
}

const FColorMap & UThemeDB::Surface() const{
  switch( this->theme ){
  case EThemeType::Light:
    return this->lightSurface;
  case EThemeType::Dark:
    return this->darkSurface;
  default:
    return this->lightSurface;
  }
}

FThemeColor UThemeDB::ColorFromStyle( const FColorStyle &style ) const{  if( this->ColorMap().Contains( style.color ) ){
    const FColorMap& colorMap = this->ColorMap()[style.color];
    if( colorMap.shades.Contains( style.shade ) ){
      return colorMap.shades[style.shade];
    }
  }
  
  return FThemeColor( FColor::Red, EThemeShades::A0 );
}

