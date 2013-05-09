#include <XBee.h>

//Below defines the byte values for all packet types
#define PING_BYTE      0x00
#define NAME_BYTE      0x01
#define MISMATCH_BYTE  0x02
#define BRAND_BYTE     0X03
#define MATCH_BYTE     0x04
#define EVENT_BYTE     0x05
#define POWER_OFF_BYTE 0x06
#define OPTION_BYTE    0x07
#define TRIGGER_BYTE   0x08
#define THROW_BYTE     0x09
#define VERSION_BYTE   0X0B

const char NAME[] = "Remote Arduino";
const char BRAND[] = "MichaelCorp";
const uint8_t VERSION = 0x04;

/* Event 1 */
char event1_name[] = "Event1";
char event1_desc[] = "Description goes here";
const int event1_id = 1;
uint8_t event1_trig = 1;
uint8_t event1_op1 = 1;
uint8_t event1_op2 = 0;
char event1_op1_desc[] = "Integer";

/* Event 2 */
char event2_name[] = "Event2";
char event2_desc[] = "This one can't be triggered";
const int event2_id = 2;
uint8_t event2_trig = 0;
uint8_t event2_op1 = 0;
uint8_t event2_op2 = 0;

/* Toggle Light */
char event3_name[] = "Toggle Light";
char event3_desc[] = "Toggle the light on or off";
const int event3_id = 3;
uint8_t event3_trig = 1;
uint8_t event3_op1 = 0;
uint8_t event3_op2 = 0;

XBee xbee = XBee();
XBeeResponse response = XBeeResponse();
Rx64Response rx64 = Rx64Response();
XBeeAddress64 addr64;

int ledPin = 13;
int currentLedStatus = LOW;

uint8_t option = 0;
uint8_t* data = 0;

bool registered;

void setup()
{
  pinMode(ledPin, OUTPUT);
  
  randomSeed(analogRead(5));
  
  registered = false;
  
  // start serial
  Serial.begin(9600);
  xbee.setSerial(Serial);
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
        uint8_t payload[2];
        payload[0] = VERSION_BYTE;
        payload[1] = VERSION;
        
        tx = Tx64Request(addr64, payload, sizeof(payload));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        break;
      case MATCH_BYTE:
        registered = true;
        break;
      case MISMATCH_BYTE:
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
        
        //ToggleLight Event information
        uint8_t payload5[sizeof(event3_name) + sizeof(event3_desc) + 5];
        byte_loc = 0;
        payload5[byte_loc++] = EVENT_BYTE;
        payload5[byte_loc++] = (uint8_t)sizeof(event3_name);
        for(int i = 0; i < sizeof(event3_name); i++)
        {
          payload5[byte_loc++] = event3_name[i];
        }
        payload5[byte_loc++] = (uint8_t)sizeof(event3_desc);
        for(int i = 0; i < sizeof(event3_desc); i++)
        {
          payload5[byte_loc++] = event3_desc[i];
        }
        
        payload5[byte_loc++] = (event3_trig << 7) | (event3_op1 << 3)
                             | (event3_op2 << 2)  | ((event3_id & 0x300) >> 8);
        payload5[byte_loc++] = event3_id & 0x00FF;
        
        tx = Tx64Request(addr64, payload5, sizeof(payload5));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        //Event1 Option1 information
        uint8_t payload6[sizeof(event1_op1_desc) + 4];
        byte_loc = 0;
        payload6[byte_loc++] = OPTION_BYTE;
        payload6[byte_loc++] = (event1_id & 0x300) >> 8;
        payload6[byte_loc++] = event1_id & 0xFF;
        payload6[byte_loc++] = (uint8_t)sizeof(event1_op1_desc);
        for(int i = 0; i < sizeof(event1_op1_desc); i++)
        {
          payload6[byte_loc++] = event1_op1_desc[i];
        }
        
        tx = Tx64Request(addr64, payload6, sizeof(payload6));
        //tx.setFrameId(0);
        xbee.send(tx);
        AwaitConfirmation();
        
        registered = true;
        
        break;
      
      case POWER_OFF_BYTE:
        registered = false;
        break;
      
      case TRIGGER_BYTE:
        byte_loc = 1;
        int evID = data[byte_loc++];
        evID = (evID << 7) | data[byte_loc++];
        
        switch(evID)
        {
          case event1_id:
            if(event1_trig > 0)
            {
              int len = data[byte_loc++];
              int integer = 1;
              
              for(int i = len-1; i >= 0; i--)
              {
                int cur = (int)data[byte_loc++];
                if(cur < 48 || cur > 57)
                  break;
                cur -= 48;
                integer += (cur * pow(10, i));
              }
              
              ThrowEvent1(integer);
            }
            break;
          
          case event2_id:
            if(event2_trig > 0)
              ThrowEvent2();
            break;
          
          case event3_id:
            if(event3_trig > 0)
              ToggleLight();
        }
        break;
    }
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
          //empty
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
    
  long randNum = random(15000);
  
  if(randNum == 0)
  {
    if(random(2) > 0)
    {
      ThrowEvent1(analogRead(random(5)));
    }
    else
    {
      ThrowEvent2();
    }
  }
}

void ThrowEvent1(int eventInt)
{
  Tx64Request tx;
  
  int byte_loc = 0;
  String myString = String(eventInt);
  
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

void ThrowEvent2()
{
  Tx64Request tx;
  
  uint8_t  payload[3];
  payload[0] = THROW_BYTE;
  payload[1] = (uint8_t)(event2_id & 0x300) >> 8;
  payload[2] = (uint8_t)(event2_id & 0xFF);
  
  tx = Tx64Request(addr64, payload, sizeof(payload));
  xbee.send(tx);
  AwaitConfirmation();
}

void ToggleLight()
{
  currentLedStatus = !currentLedStatus;
  
  digitalWrite(ledPin, currentLedStatus);
  
  Tx64Request tx;
  
  uint8_t  payload[3];
  payload[0] = THROW_BYTE;
  payload[1] = (uint8_t)(event3_id & 0x300) >> 8;
  payload[2] = (uint8_t)(event3_id & 0xFF);
  
  tx = Tx64Request(addr64, payload, sizeof(payload));
  xbee.send(tx);
  AwaitConfirmation();
}
