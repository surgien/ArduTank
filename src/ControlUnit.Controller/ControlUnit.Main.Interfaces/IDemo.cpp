#include "stdafx.h"
#include "IDemo.h"


IDemo::IDemo()
{
}


IDemo::~IDemo()
{
}

#pragma once
class IBase {
public:
	virtual ~IBase() {}; // destructor, use it to call destructor of the inherit classes
	virtual void Describe() = 0; // pure virtual method
};
#pragma once
class Tester : public IBase {
public:
	Tester();
	virtual ~Tester();
	virtual void Describe();
};

Tester::Tester() {
}

Tester::~Tester() {
}

void Tester::Describe() {
}


void descriptor(IBase * obj) {
	obj->Describe();
}

int main(int argc, char** argv) {



	return 0;
}