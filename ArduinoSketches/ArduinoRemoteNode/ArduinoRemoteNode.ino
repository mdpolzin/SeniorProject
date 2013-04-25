#include <XBee.h>s

//Below defines the byte values for received packet types
#define PING_BYTE    0x00
#define NAME_BYTE    0x01
#define BRAND_BYTE   0X03

const char NAME[] = "Remote Arduino";
const char BRAND[] = "MichaelCorp";

XBee xbee = XBee();
XBeeResponse response = XBeeResponse();
Rx64Response rx64 = Rx64Response();

int statusLed = 11;
int errorLed = 12;
int dataLed = 10;

uint8_t option = 0;
uint8_t* data = 0;

void flashLed(int pin, int times, int wait)
{    
  for (int i = 0; i < times; i++)
  {
    digitalWrite(pin, HIGH);
    delay(wait);
    digitalWrite(pin, LOW);
    
    if (i + 1 < times)
    {
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
      HandleXbeePacket();
    }
}

void HandleXbeePacket()
{
  if (xbee.getResponse().getApiId() == RX_64_RESPONSE)
  {
    // got a rx64bit packet
    Tx64Request tx;
    
    xbee.getResponse().getRx64Response(rx64);
    XBeeAddress64 addr64 = rx64.getRemoteAddress64();
    option = rx64.getOption();
    data = rx64.getData();
    
    switch(data[0])
    {
      case PING_BYTE:
        uint8_t payload1[sizeof(NAME) + 1];
        payload1[0] = NAME_BYTE;
        for(int i = 1 ; i < sizeof(payload1); i++)
        {
          payload1[i] = (uint8_t)NAME[i-1];
        }
        tx = Tx64Request(addr64, payload1, sizeof(payload1));
        xbee.send(tx);
        AwaitConfirmation();
        
        uint8_t payload2[sizeof(BRAND) + 1];
        payload2[0] = BRAND_BYTE;
        for(int i = 1 ; i < sizeof(payload2); i++)
        {
          payload2[i] = (uint8_t)BRAND[i-1];
        }
        tx = Tx64Request(addr64, payload2, sizeof(payload2));
        xbee.send(tx);
        AwaitConfirmation();
        
        break;
      default:
        break;
    }
    
    
  }
  else
  {
    // not something we were expecting
    flashLed(errorLed, 1, 25);    
  } 
}

void AwaitConfirmation()
{
  TxStatusResponse txStatus = TxStatusResponse();
  
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
    
  }
  else
  {
    // local XBee did not provide a timely TX Status Response.  Radio is not configured properly or connected
    flashLed(errorLed, 2, 50);
  }
}
