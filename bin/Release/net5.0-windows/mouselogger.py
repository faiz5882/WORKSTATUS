from pynput import mouse
import logging
import pynput
import os

cwd = os.getcwd()
log_directory = os.path.join(cwd,"Key_logs")
if not os.path.exists(log_directory):
    os.makedirs(log_directory)

i = 0
while os.path.exists("mouse_log%s.txt" %i):
    i += 1

formatter = logging.Formatter('%(asctime)s %(message)s')


def setup_logger(name, log_file, level=logging.INFO):
    handler = logging.FileHandler(log_file)        
    handler.setFormatter(formatter)

    logger = logging.getLogger(name)
    logger.setLevel(level)
    logger.addHandler(handler)
    return logger

mouse_logger = setup_logger('mouse_logger', 'mouse_log%s.txt' %i)


def on_move(x, y):
    mouse_logger.info('Pointer-moved-to {0}'.format((x, y)))

def on_click(x, y, button, pressed):
    mouse_logger.info('{0}-at {1} '.format('Pressed' if pressed else 'Released',(x, y)))

def on_scroll(x, y, dx, dy):
    mouse_logger.info('Scrolled-{0}-at {1} '.format('down' if dy < 0 else 'up',(x, y)))


# Collect events until released
with mouse.Listener(on_move=on_move, on_click=on_click, on_scroll=on_scroll) as listener_m:
    listener_m.join()
