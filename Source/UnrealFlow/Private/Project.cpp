#include "Project.h"

UProject::UProject(){
  this->displayName = "";
  this->projectPath = "";
  this->syncName = "";
  this->versionsToKeep = 0;
  this->syncPaths = TArray<FString>();
}
