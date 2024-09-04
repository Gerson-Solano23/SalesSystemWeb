from statsmodels.tsa.holtwinters import ExponentialSmoothing


import numpy as np
import matplotlib.pyplot as plt
import pyodbc
connection_string = 'DRIVER={SQL Server};SERVER=LENOVO-GERS\\SQLEXPRESS;DATABASE=MySalesCompany;Trusted_Connection=True; TrustServerCertificate=True"'
conn = pyodbc.connect(connection_string)
cursor = conn.cursor()
# Ejecuta el procedimiento almacenado
cursor.execute("{CALL CSP_GET_TotalsPerMonth}")
rows = cursor.fetchall()

x = np.array([float(row[0]) for row in rows])
y = np.array([float(row[1]) for row in rows])

cursor.close()
conn.close()

# Ahora puedes usar x e y en tu código
print("Valores de x:", x)
print("Valores de y:", y)

mes = 9.
coef = np.polyfit(x,y,1)    
p = np.polyval(coef, mes)
print("El valor de la población en el mes",mes,"es:",p)
# x= np.array([5,6,7,8,9])
# y= np.array([215630.00,175630.00,235630.00,235630.00,114840.00])


# Modelado con Holt-Winters Exponential Smoothing
model = ExponentialSmoothing(y, trend="add", seasonal=None, seasonal_periods=12)
model_fit = model.fit()
predictions = model_fit.predict(start=len(y), end=len(y) + len(y) - 1)

# Graficar resultados
plt.figure(figsize=(12, 6))
plt.plot(np.arange(1, len(y)+1), y, label='Valores reales')
plt.plot(np.arange(len(y)+1, len(y)+len(predictions)+1), predictions, label='Predicción')
plt.xlabel('Meses')
plt.ylabel('Totales')
plt.title('Predicción de Totales Mensuales')
plt.legend()
plt.show()


# for i in range(1, 10):
#     coef = np.polyfit(x,y,1)    
#     p = np.polyval(coef, mes)

#     print(f"Para grado: {i} la prediccion es de: {p}")
#     X1 = np.linspace(5, mes+1, 1000)
#     Y1 = np.polyval(coef, X1)
#     plt.figure(figsize=(20, 10))
#     plt.title(f"Cantidades Totales por mes para el Grado:" + str(i))

#     plt.scatter(x, y, s=120, color='red')  
#     plt.plot(X1, Y1, "--", linewidth=3, color='orange')    
#     plt.scatter(mes, p, s=120, color='blue')    
#     plt.yticks(np.arange(0, 300000, 10000))
#     plt.grid('on')

#     ax=plt.gca()
#     ax.set_xlabel('Mes')
#     ax.set_ylabel('Cantidad Total')

#     plt.show()


    