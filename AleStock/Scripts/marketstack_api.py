import json
from urllib.request import Request, urlopen
from datetime import date as datemethod
from datetime import timedelta

class MarketStackAPI:
    
    # initialize marketstack class using api key
    def __init__(self, key):
        self.key = key

    def build_url_eod(self, ticker, date_start, date_end):
        # create param string to append to url for GET request
        params = f'?symbols={ticker}&date_from={date_start}&date_to={date_end}&sort=asc&access_key={self.key}'
        # create link to backend with params appended
        req_link = 'https://api.marketstack.com/v1/eod{}'.format(params)
        
    def build_url_intraday(self, ticker, date_start, date_end):
        # create param string to append to url for GET request
        params = f'?symbols={ticker}&date_from={date_start}&date_to={date_end}&sort=asc&access_key={self.key}'
        # create link to backend with params appended
        req_link = 'https://api.marketstack.com/v1/intraday{}'.format(params)