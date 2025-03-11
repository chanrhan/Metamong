# 텍스트 임베딩 모델인 FastText의 모델을 불러오기 위한 API 
# FastText는 한글을 지원한다 하여 사용해보려고 했는데, 일단 보류
# 한글 사용을 원하면 해당 API를 통해 모델을 불러와서 사용해보면 될듯 (사용법은 구글링... 해주세용)
# 다운받은 모델의 용량은 총 11GB를 넘어가니... Git Push 시 포함하지 않기 바람 (어차피 용량크면 Push 안되긴 할겁니다)

import fasttext.util

fasttext.util.download_model('ko',if_exists='ignore')
ft = fasttext.load_model('cc.ko.300.bin')