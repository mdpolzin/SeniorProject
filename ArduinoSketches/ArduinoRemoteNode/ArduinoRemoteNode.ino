#include <XBee.h>

//Below defines the byte values for received packet types
#define PING_BYTE      0x00
#define NAME_BYTE      0x01
#define POWER_OFF_BYTE 0x02
#define BRAND_BYTE     0X03
#define EVENT_BYTE     0x05
#define OPTION_BYTE    0x07
#define THROW_BYTE     0x09

const char NAME[] = "Remote Arduino";
const char BRAND[] = "MichaelCorp";

/* Event 1 */
char event1_name[] = "Event1";
char event1_desc[] = "Description goes here";
int event1_id = 1;
uint8_t event1_trig = 1;
uint8_t event1_op1 = 1;
uint8_t event1_op2 = 0;
char event1_op1_desc[] = "Integer";

/* Event 2 */
char event2_name[] = "Event2";
char event2_desc[] = "This one can't be triggered";
int event2_id = 2;
uint8_t event2_trig = 0;
uint8_t event2_op1 = 0;
uint8_t event2_op2 = 0;

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
    int byte_loc = 0;
    
    xbee.getResponse().getRx64Response(rx64);
    addr64 = rx64.getRemoteAddress64();
    option = rx64.getOption();
    data = rx64.getData();
    
    switch(data[0])
    {
      case PING_BYTE:
        //Send Name information followed by Brand information followed by Event information
        uint8_t payload1[sizeof(NAME) + 1];
        byte_loc = 0;
        payload1[byte_loc++] = NAME_BYTE;
        for(int i = 1 ; i < sizeof(payload1); i++)
        {
          payload1[byte_loc++] = (uint8_t)NAME[i-1];
        }
        tx = Tx64Request(addr64, payload1, sizeof(payload1));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        //Brand information
        uint8_t payload2[sizeof(BRAND) + 1];
        byte_loc = 0;
        payload2[byte_loc++] = BRAND_BYTE;
        for(int i = 1 ; i < sizeof(payload2); i++)
        {
          payload2[byte_loc++] = (uint8_t)BRAND[i-1];
        }
        tx = Tx64Request(addr64, payload2, sizeof(payload2));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        //Event1 information
        uint8_t payload3[sizeof(event1_name) + sizeof(event1_desc) + 5];
        byte_loc = 0;
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
        
        payload3[byte_loc++] = (event1_trig << 7) | (event1_op1 << 3)
                             | (event1_op2 << 2)  | ((event1_id & 0x300) >> 8);
        payload3[byte_loc++] = event1_id & 0x00FF;
        
        tx = Tx64Request(addr64, payload3, sizeof(payload3));
        //tx.setFrameId(0);
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
        
        payload4[byte_loc++] = (event2_trig << 7) | (event2_op1 << 3)
                             | (event2_op2 << 2)  | ((event2_id & 0x300) >> 8);
        payload4[byte_loc++] = event2_id & 0x00FF;
        
        tx = Tx64Request(addr64, payload4, sizeof(payload4));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        //Event1 Option1 information
        uint8_t payload5[sizeof(event1_op1_desc) + 4];
        byte_loc = 0;
        payload5[byte_loc++] = OPTION_BYTE;
        payload5[byte_loc++] = (event1_id & 0x300) >> 8;
        payload5[byte_loc++] = event1_id & 0xFF;
        payload5[byte_loc++] = (uint8_t)sizeof(event1_op1_desc);
        for(int i = 0; i < sizeof(event1_op1_desc); i++)
        {
          payload5[byte_loc++] = event1_op1_desc[i];
        }
        
        tx = Tx64Request(addr64, payload5, sizeof(payload5));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        registered = true;
        
        break;
      
      case POWER_OFF_BYTE:
        registered = false;
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
            registered = false;
        }
    }
  }
  else
  {
    // local XBee did not provide a timely TX Status Response.  Radio is not configured properly or connected
    registered = false;
  }
}

void ThrowEvent()
{
  if(!registered)
    return;
    
  long randNum = random(10000);
  
  if(randNum == 0)
  {
    Tx64Request tx;
    
    if(random(2) > 0)
    {
      int byte_loc = 0;
      String myString = String(analogRead(5));
      Serial.println(myString);
      uint8_t  payload[myString.length() + 4];
      payload[byte_loc++] = THROW_BYTE;
      payload[byte_loc++] = (uint8_t)(event1_id & 0x300) >> 8;
      payload[byte_loc++] = (uint8_t)(event1_id & 0xFF);

      payload[byte_loc++] = myString.length();
      
      for(int i = 0; i < myString.length(); i++)
      {
        payload[byte_loc++] = (char)myString[i];
      }
      
      tx = Tx64Request(addr64, payload, sizeof(payload));
      xbee.send(tx);
      AwaitConfirmation();
    }
    else
    {
      uint8_t  payload[3];
      payload[0] = THROW_BYTE;
      payload[1] = (uint8_t)(event2_id & 0x300) >> 8;
      payload[2] = (uint8_t)(event2_id & 0xFF);
      
      tx = Tx64Request(addr64, payload, sizeof(payload));
      xbee.send(tx);
      AwaitConfirmation();
    }
  }
}
