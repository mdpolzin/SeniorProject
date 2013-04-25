#include <XBee.h>

//Below defines the byte values for received packet types
#define PING_BYTE    0x00
#define NAME_BYTE    0x01
#define BRAND_BYTE   0X03
#define EVENT_BYTE   0x05
#define THROW_BYTE   0x07

const char NAME[] = "Remote Arduino";
const char BRAND[] = "MichaelCorp";

/* Event 1 */
char event1_name[] = "Event1";
char event1_desc[] = "Description goes here";
uint8_t event1_id = 1;
uint8_t event1_trig = 1;

/* Event 2 */
char event2_name[] = "Event2";
char event2_desc[] = "This one can't be triggered";
uint8_t event2_id = 2;
uint8_t event2_trig = 0;

XBee xbee = XBee();
XBeeResponse response = XBeeResponse();
Rx64Response rx64 = Rx64Response();
XBeeAddress64 addr64;

int statusLed = 11;
int errorLed = 12;
int dataLed = 10;

uint8_t option = 0;
uint8_t* data = 0;

bool registered;

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
  
  randomSeed(analogRead(5));
  
  registered = false;
  
  // start serial
  Serial.begin(9600);
  xbee.setSerial(Serial);
  
  flashLed(statusLed, 3, 50);
}

// continuously reads packets, looking for RX64
// Also randomly throws events
void loop()
{
    xbee.readPacket();
    
    if (xbee.getResponse().isAvailable())
    {
      HandleXbeePacket();
    }
    
    ThrowEvent();
}

void HandleXbeePacket()
{
  if (xbee.getResponse().getApiId() == RX_64_RESPONSE)
  {
    // got a rx64bit packet
    Tx64Request tx;
    
    xbee.getResponse().getRx64Response(rx64);
    addr64 = rx64.getRemoteAddress64();
    option = rx64.getOption();
    data = rx64.getData();
    
    switch(data[0])
    {
      case PING_BYTE:
        //Send Name information followed by Brand information followed by Event information
        uint8_t payload1[sizeof(NAME) + 1];
        payload1[0] = NAME_BYTE;
        for(int i = 1 ; i < sizeof(payload1); i++)
        {
          payload1[i] = (uint8_t)NAME[i-1];
        }
        tx = Tx64Request(addr64, payload1, sizeof(payload1));
        xbee.send(tx);
        AwaitConfirmation();
        
        //Brand information
        uint8_t payload2[sizeof(BRAND) + 1];
        payload2[0] = BRAND_BYTE;
        for(int i = 1 ; i < sizeof(payload2); i++)
        {
          payload2[i] = (uint8_t)BRAND[i-1];
        }
        tx = Tx64Request(addr64, payload2, sizeof(payload2));
        xbee.send(tx);
        AwaitConfirmation();
        
        //Event1 information
        uint8_t payload3[sizeof(event1_name) + sizeof(event1_desc) + 5];
        int byte_loc = 0;
        payload3[byte_loc++] = EVENT_BYTE;
        payload3[byte_loc++] = (uint8_t)sizeof(event1_name);
        for(int i = 0; i < sizeof(event1_name); i++)
        {
          payload3[byte_loc++] = event1_name[i];
        }
        payload3[byte_loc++] = (uint8_t)sizeof(event1_desc);
        for(int i = 0; i < sizeof(event1_desc); i++)
        {
          payload3[byte_loc++] = event1_desc[i];
        }
        payload3[byte_loc++] = event1_id;
        payload3[byte_loc++] = event1_trig;
        
        tx = Tx64Request(addr64, payload3, sizeof(payload3));
        xbee.send(tx);
        AwaitConfirmation();
        
        //Event2 information
        uint8_t payload4[sizeof(event2_name) + sizeof(event2_desc) + 5];
        byte_loc = 0;
        payload4[byte_loc++] = EVENT_BYTE;
        payload4[byte_loc++] = (uint8_t)sizeof(event2_name);
        for(int i = 0; i < sizeof(event2_name); i++)
        {
          payload4[byte_loc++] = event2_name[i];
        }
        payload4[byte_loc++] = (uint8_t)sizeof(event2_desc);
        for(int i = 0; i < sizeof(event2_desc); i++)
        {
          payload4[byte_loc++] = event2_desc[i];
        }
        payload4[byte_loc++] = event2_id;
        payload4[byte_loc++] = event2_trig;
        
        tx = Tx64Request(addr64, payload4, sizeof(payload4));
        xbee.send(tx);
        AwaitConfirmation();
        
        registered = true;
        
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

void ThrowEvent()
{
  if(!registered)
    return;
    
  long randNum = random(1000);
  
  if(randNum == 0)
  {
    Tx64Request tx;
    uint8_t  payload[2];
    payload[0] = THROW_BYTE;
    
    if(random(2) == 0)
    {
      payload[1] = event1_id;
    }
    else
    {
      payload[1] = event2_id;
    }
    
    tx = Tx64Request(addr64, payload, sizeof(payload));
    xbee.send(tx);
    AwaitConfirmation();
  }
}