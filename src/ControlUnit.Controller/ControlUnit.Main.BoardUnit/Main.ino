#include <BLEPeripheral.h>
#include <BLESerial.h>

// create ble serial instance
BLESerial bleSerial = BLESerial();


void setup() {
  // custom services and characteristics can be added as well
  bleSerial.setLocalName("ArduTank");

  //initialize BLE led
  pinMode(BLE_LED, OUTPUT);

  Serial.begin(9600);
  bleSerial.begin();
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


// forward received from Serial to BLESerial and vice versa
void forward() {
  if (bleSerial && Serial) {
    int byte;
    while ((byte = bleSerial.read()) > 0) Serial.write((char)byte);
    while ((byte = Serial.read()) > 0) bleSerial.write((char)byte);
  }
}