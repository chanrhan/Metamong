import fasttext.util

fasttext.util.download_model('ko',if_exists='ignore')
ft = fasttext.load_model('cc.ko.300.bin')