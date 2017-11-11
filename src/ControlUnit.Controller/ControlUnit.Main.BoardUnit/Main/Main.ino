#include <BLEPeripheral.h>
#include <BLESerial.h>
#include <Wire.h>
#include <Adafruit_MotorShield.h>
#include "utility/Adafruit_MS_PWMServoDriver.h"

// create ble serial instance
BLESerial bleSerial = BLESerial();
// Create the motor shield object with the default I2C address
Adafruit_MotorShield AFMS = Adafruit_MotorShield(); 
// Or, create it with a different I2C address (say for stacking)
// Adafruit_MotorShield AFMS = Adafruit_MotorShield(0x61); 

// Select which 'port' M1, M2, M3 or M4. In this case, M1
Adafruit_DCMotor *leftMotor = AFMS.getMotor(1);
Adafruit_DCMotor *rightMotor = AFMS.getMotor(2);
// You can also make another motor on port M2
//Adafruit_DCMotor *myOtherMotor = AFMS.getMotor(2);

void setup() {
  // custom services and characteristics can be added as well
  bleSerial.setLocalName("ArduTank");

  //initialize BLE led
  pinMode(BLE_LED, OUTPUT);

  Serial.begin(9600);
  bleSerial.begin();
  AFMS.begin();  // create with the default frequency 1.6KHz
  //AFMS.begin(1000);  // OR with a different frequency, say 1KHz
  // leftMotor->setSpeed(150);
  // rightMotor->setSpeed(150);

  // // Set the speed to start, from 0 (off) to 255 (max speed)
  // leftMotor->run(FORWARD);
  // rightMotor->run(FORWARD);
  // // turn on motor
  // leftMotor->run(RELEASE);
  // rightMotor->run(RELEASE);
}

void loop() {
   bleSerial.poll();

  forward();

  // handle the BLE led. Blink when advertising
  if(bleSerial.status() == ADVERTISING){
    digitalWrite(BLE_LED, LOW);
    delay(200);
    digitalWrite(BLE_LED, HIGH);
    delay(200);
  }
  else // if we are not advertising, we are connected
  digitalWrite(BLE_LED, HIGH);
}

bool containsEnd = false;
bool needNextSeq = false;
String currentValue="";

// forward received from Serial to BLESerial and vice versa
void forward() {
  if (bleSerial && Serial) {
    int val;
    // String test = "hiii";

    // Serial.println(test);
    char inputValue[50];
    int i=0;
    // Serial.println(bleSerial.readString());
    while ((val = bleSerial.read()) > 0)
    {
      char c =(char)val;
      // Serial.write(c);
      inputValue[i]=c;
      i++;
    }
    if (i>0)
    {
      // inputValue[i]=0; //Evtl. null terminator
      char parsedValue[i];
      containsEnd=false;
      for(int j =0;j<i;j++)
      {
        parsedValue[j]=inputValue[j];

        if(parsedValue[j]==')') containsEnd=true;
      }
      
      currentValue+=parsedValue;
      
      if(containsEnd) 
      {
        Serial.println(currentValue);

        int methodEnd=currentValue.indexOf('(');
        String methodName=currentValue.substring(1,methodEnd);
        String paramValue=currentValue.substring(methodEnd+1,currentValue.indexOf(')'));
        float param = paramValue.toFloat();

        Serial.print("Methode: ");
        Serial.println(methodName);
        
        Serial.print("Parameter: ");
        Serial.print(param);
        Serial.println(paramValue);

        if(methodName == "Accelerate")  Accelerate(param);
        else if(methodName == "AccelerateLeftTrack") AccelerateLeftTrack(param);
        else if(methodName == "AccelerateRightTrack") AccelerateRightTrack(param);

        // switch (methodName) {
        //   case "Accelerate":
        //     Accelerate(33.3);
        //     break;
        //   case "AccelerateLeftTrack":
        //     AccelerateLeftTrack(33.3);
        //     break;
        //   case "AccelerateRightTrack":
        //     AccelerateRightTrack(33.3);
        //     break;
        // }

        currentValue="";
      }
      else{
        // Serial.println("need next");
      }
    }
  }
}

void Accelerate(float value)
{
  leftMotor->run(FORWARD);
  rightMotor->run(FORWARD);
  int val=(254/100)*value;
  leftMotor->setSpeed(val);
  rightMotor->setSpeed(val);
}

void AccelerateLeftTrack(float value)
{
  leftMotor->run(FORWARD);
  leftMotor->setSpeed((254/100)*value);
}

void AccelerateRightTrack(float value)
{
  rightMotor->run(FORWARD);
  rightMotor->setSpeed((254/100)*value);
}
