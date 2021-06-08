# MeshSparklyEffect ドキュメント

## 使い方

0. Project SettingsのGraphicsでScriptable Render Pipeline SettingsにUniversal Render Pipeline Assetが正しくセットされていることを確認
1. GameObjectにMeshSparklyEffect.csを追加
1. The SkinnedMeshRenderer is missing.というエラーとともに、Switch MeshFilterボタンとSkinned Mesh Rendererを指定するプロパティが現れる
1. ここにSkinned Mesh Rendererを指定することでエフェクトを調整するパラメータが現れる（Switch MeshFilter Modeボタンを押すことでMeshFilterを指定することも可能）
1. MeshSparklyEffectのインスペクタにエフェクトを調整するパラメータが現れたら、各プロパティを調整することでエフェクトの見た目を変えることができる

## UIの各項目の説明

![インスペクター画面](./Images/inspector.png)

### Switch MeshFilter/SkinnedMeshRenderer Mode ボタン

このボタンを押すことでSkinned Mesh RendererとMeshFilterのどちらを使用するかを切り替えることができる。

また、切り替え先のMeshが登録されていなかった場合はインスペクタ上部にエラーが表示されて各パラメータが隠された初期状態に戻る。

### Target SkinnedMeshRenderer/MeshFilter

対象となるメッシュの指定。ここに指定したメッシュの頂点情報を元にパーティクルの位置を決定する。

ただし、ここの指定だけではMeshのTransformには追従しないので、下層に追加されるVisual
Effectの付いたGameObjectにConstraintを追加するなどして追従させる必要がある（一例として、MeshFilterというサンプルシーンではParent Constraintを使用している）

### Color Texture

パーティクルに色を付けるために使うテクスチャ。指定したメッシュのUVを使用してテクスチャ上の色をパーティクルに反映するため、指定しているメッシュで使用しているテクスチャを使用するとパーティクルが出た頂点位置に対応した色が反映される。

### Rate

パーティクルが出る頻度。この値が大きいほど、短時間に多くのパーティクルが出てくる。

### Alpha

パーティクルのAlpha値。

### Size Decay Curve

パーティクルのサイズがLife Timeに応じてどのように変化するかを指定するAnimation Curve。実際のパーティクルサイズはSize Min-Maxで指定された範囲内のランダムな値と、このSize Decay Curveをかけた値で決定する。

### Size Min-Max

ランダムに決定されるパーティクルのサイズの範囲を指定する。MinとMaxを同じ値にすることで、一意のサイズに固定することも可能。

また、このMinMaxSliderはLow LimitとHigh Limitを変更することで、スライダーで動かすことが可能な最小値と最大値を指定することができる。

### Life Time Min-Max

ランダムに決定されるパーティクルの表示時間の範囲を指定する。Size Min-Maxと同じく、同じ値にすることで固定可能。

### Emission Intensity

パーティクルのEmission強度。

### Rotate Degree

パーティクルの回転角を度数法で指定する。

### Offset

パーティクルの出現位置を、メッシュの頂点位置から法線方向に向かってどれだけずらすかを指定する。この値を調整してパーティクルがメッシュと重ならないように調整する。

### Switch Texture/Procedural Mode ボタン

このボタンは、十字形のプロシージャルなパーティクルを使用するか、テクスチャをパーティクルとして使用するかを切り替えることができる。

このボタンを押してモードを切り替えると、現在のモードに合ったパラメータが表示される。

### Spike Width (Procedural Mode)

十字形のパーティクルのスパイクの太さを指定する。

### Sparkle Texture (Texture Mode)

パーティクルとして使用するテクスチャを指定する。

### Convert to Map/Mesh ボタン

メッシュの頂点情報から生成したテクスチャをインスペクタに表示する。Convert to Meshとボタンに表示されているときに再度押すことで元のMeshを表示するモードに戻すことができる。

![Convert to Map画面](./Images/vertex_map.png)

Bakeボタンを押すと、各テクスチャを保存するディレクトリを指定するためのWindowが表示され、指定した場所にメッシュの名前とそれぞれのマップ名の組み合わせをファイル名としたEXRファイルとして保存される。