/**
 * Copyright (c) 2009 Andrew Rapp. All rights reserved.
 *
 * This file is part of XBee-Arduino.
 *
 * XBee-Arduino is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * XBee-Arduino is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with XBee-Arduino.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <XBee.h>

/*
This example is for Series 1 XBee (802.15.4)
Receives either a RX16 or RX64 packet and sets a PWM value based on packet data.
Error led is flashed if an unexpected packet is received
*/

XBee xbee = XBee();
XBeeResponse response = XBeeResponse();
// create reusable response objects for responses we expect to handle 
Rx16Response rx16 = Rx16Response();
Rx64Response rx64 = Rx64Response();

int statusLed = 11;
int errorLed = 12;
int dataLed = 10;

uint8_t option = 0;
uint8_t* data = 0;

void flashLed(int pin, int times, int wait) {
    
    for (int i = 0; i < times; i++) {
      digitalWrite(pin, HIGH);
      delay(wait);
      digitalWrite(pin, LOW);
      
      if (i + 1 < times) {
        delay(wait);
      }
    }
}

void setup() {
  pinMode(statusLed, OUTPUT);
  pinMode(errorLed, OUTPUT);
  pinMode(dataLed,  OUTPUT);
  
  // start serial
  Serial.begin(9600);
  xbee.setSerial(Serial);
  
  flashLed(statusLed, 3, 50);
}

// continuously reads packets, looking for RX16 or RX64
void loop()
{
    xbee.readPacket();
    
    if (xbee.getResponse().isAvailable())
    {
      // got something
      Serial.print("Got Something!\n");
      
      if (xbee.getResponse().getApiId() == RX_64_RESPONSE)
      {
        // got a rx packet
        
          xbee.getResponse().getRx64Response(rx64);
          option = rx64.getOption();
          data = rx64.getData();
              
          uint8_t payload[xbee.getResponse().getPacketLength() - 5];
          // Print packet data to Serial
          for( int i = 0; i < xbee.getResponse().getPacketLength() - 5; i++)
          {
              Serial.print((char)data[i]);
              payload[i] = data[i];
          }

          XBeeAddress64 addr64 = XBeeAddress64(0x13A200, 0x409FCEAC);
          Tx64Request tx = Tx64Request(addr64, payload, sizeof(payload));
          TxStatusResponse txStatus = TxStatusResponse();
          
          xbee.send(tx);
          
          if (xbee.readPacket(5000))
          {
          // got a response!
      
              // should be a znet tx status            	
              if (xbee.getResponse().getApiId() == TX_STATUS_RESPONSE)
              {
                  xbee.getResponse().getZBTxStatusResponse(txStatus);
          		
                  // get the delivery status, the fifth byte
                  if (txStatus.getStatus() == SUCCESS)
                  {
                      // success.  time to celebrate
                      flashLed(statusLed, 5, 50);
                  }
                  else
                  {
                      // the remote XBee did not receive our packet. is it powered on?
                      flashLed(errorLed, 3, 500);
                  }
              }      
          }
          else if (xbee.getResponse().isError())
          {
            //nss.print("Error reading packet.  Error code: ");  
            //nss.println(xbee.getResponse().getErrorCode());
            // or flash error led
          }
          else
          {
            // local XBee did not provide a timely TX Status Response.  Radio is not configured properly or connected
            flashLed(errorLed, 2, 50);
          }
          
          
          
      }
      else
      {
      	  // not something we were expecting
          flashLed(errorLed, 1, 25);    
      }
    }
    else if (xbee.getResponse().isError())
    {
      //nss.print("Error reading packet.  Error code: ");  
      //nss.println(xbee.getResponse().getErrorCode());
      // or flash error led
    } 
}
